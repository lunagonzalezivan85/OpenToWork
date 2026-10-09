// Mapa del selector de ubicacion de /vacancies: Leaflet + MapLibre GL (tiles vectoriales
// OpenFreeMap) via leaflet-maplibre-gl. Geocodificacion directa a Nominatim desde el navegador.
// La variante CSP de maplibre (maplibre-gl-csp.js + worker mismo origen) respeta la CSP de web.config.
(function () {
    var NOMINATIM = 'https://nominatim.openstreetmap.org';
    var map = null;
    var marker = null;
    var circle = null;
    var dotNet = null;
    var picked = null; // { lat, lng }

    function pinIcon() {
        return L.divIcon({
            className: 'otw-pin',
            html: '<span class="otw-pin-dot"></span><span class="otw-pin-ring"></span>',
            iconSize: [26, 26],
            iconAnchor: [13, 13]
        });
    }

    function setRadiusCircle(lat, lng, radiusKm) {
        if (!map) return;
        var meters = Math.max(1, (radiusKm || 0)) * 1000;
        if (!circle) {
            circle = L.circle([lat, lng], {
                radius: meters,
                className: 'otw-radius-circle',
                interactive: false
            }).addTo(map);
        } else {
            circle.setLatLng([lat, lng]);
            circle.setRadius(meters);
        }
    }

    function placeMarker(lat, lng, radiusKm) {
        picked = { lat: lat, lng: lng };
        if (!marker) {
            marker = L.marker([lat, lng], { icon: pinIcon(), draggable: true }).addTo(map);
            marker.on('dragend', function () {
                var p = marker.getLatLng();
                onPick(p.lat, p.lng);
            });
        } else {
            marker.setLatLng([lat, lng]);
        }
        setRadiusCircle(lat, lng, radiusKm);
    }

    async function onPick(lat, lng) {
        if (circle) circle.setLatLng([lat, lng]);
        picked = { lat: lat, lng: lng };
        if (!dotNet) return;
        var label = await window.otwMap.reverse(lat, lng);
        await dotNet.invokeMethodAsync('OnMapPicked', lat, lng, label || '');
    }

    function shortLabel(address, displayName) {
        if (address) {
            var place = address.city || address.town || address.village
                || address.municipality || address.county || address.suburb;
            if (place) {
                var region = address.state || address.province || '';
                return region && region !== place ? place + ', ' + region : place;
            }
        }
        return (displayName || '').split(',')[0].trim();
    }

    window.otwMap = {
        // center: {lat,lng}|null, zoom int, styleUrl string, radiusKm number, dotNetRef
        init: function (elId, center, zoom, styleUrl, radiusKm, dotNetRef) {
            var el = document.getElementById(elId);
            if (!el || typeof L === 'undefined' || typeof maplibregl === 'undefined') return false;
            this.destroy();
            dotNet = dotNetRef || null;
            picked = center || null;

            map = L.map(elId, {
                center: center ? [center.lat, center.lng] : [40.0, -3.7], // Espana
                zoom: center ? Math.max(zoom || 10, 10) : 6,
                zoomControl: true,
                attributionControl: true
            });
            map.attributionControl.setPrefix(false);

            // La build CSP de MapLibre no puede crear blob workers: hay que apuntarle
            // al worker vendoreado. Sin esto pide la URL por defecto -> 404 -> HTML
            // -> "Unexpected token '<'" (y puede tumbar la app en algunos navegadores).
            if (typeof maplibregl.setWorkerUrl === 'function') {
                maplibregl.setWorkerUrl('/lib/maplibre-gl/maplibre-gl-csp-worker.js');
            }

            L.maplibreGL({
                style: styleUrl,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &middot; <a href="https://openfreemap.org">OpenFreeMap</a>'
            }).addTo(map);

            if (center) placeMarker(center.lat, center.lng, radiusKm);

            map.on('click', function (e) {
                var p = e.latlng;
                placeMarker(p.lat, p.lng, window.otwMap._radiusKm || 0);
                onPick(p.lat, p.lng);
            });
            return true;
        },

        // Busqueda hacia adelante: devuelve [{label, lat, lng}]
        search: async function (query, countryCodes) {
            var q = (query || '').trim();
            if (q.length < 3) return [];
            try {
                var url = NOMINATIM + '/search?format=jsonv2&limit=5&accept-language=es&addressdetails=1&q='
                    + encodeURIComponent(q);
                if (countryCodes) url += '&countrycodes=' + encodeURIComponent(countryCodes);
                var res = await fetch(url);
                if (!res.ok) return [];
                var data = await res.json();
                return data.map(function (h) {
                    return {
                        label: shortLabel(h.address, h.display_name),
                        full: h.display_name,
                        lat: parseFloat(h.lat),
                        lng: parseFloat(h.lon)
                    };
                });
            } catch { return []; }
        },

        reverse: async function (lat, lng) {
            try {
                var res = await fetch(NOMINATIM + '/reverse?format=jsonv2&accept-language=es&addressdetails=1&lat='
                    + lat + '&lon=' + lng);
                if (!res.ok) return '';
                var d = await res.json();
                return shortLabel(d.address, d.display_name);
            } catch { return ''; }
        },

        goTo: function (lat, lng, radiusKm) {
            if (!map) return;
            placeMarker(lat, lng, radiusKm);
            map.flyTo([lat, lng], Math.max(map.getZoom(), 11));
            picked = { lat: lat, lng: lng };
        },

        setRadius: function (km) {
            this._radiusKm = km;
            if (picked) setRadiusCircle(picked.lat, picked.lng, km);
        },

        // reencuadra para que quepa el circulo del radio
        fitRadius: function (km) {
            if (!map || !picked || !circle) return;
            map.flyToBounds(circle.getBounds());
        },

        picked: function () { return picked; },

        destroy: function () {
            if (map) { map.remove(); }
            map = null; marker = null; circle = null; picked = null; dotNet = null;
        }
    };
})();
