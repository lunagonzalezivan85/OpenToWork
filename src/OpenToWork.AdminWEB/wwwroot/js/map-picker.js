window.mapPicker = {
    _map: null,
    _marker: null,
    _selected: null,
    // Promesa de la ultima busqueda/geocodificacion en curso: "Usar ubicacion" la espera para no
    // devolver una direccion vacia si el usuario hace clic antes de que responda Nominatim.
    _pending: null,
    _addressLabelId: null,

    openMap: function (containerId, lat, lng) {
        const el = document.getElementById(containerId);
        if (!el) return;

        if (this._map) {
            this._map.remove();
            this._map = null;
            this._marker = null;
        }
        this._selected = null;
        this._pending = null;

        // Etiqueta opcional "<containerId>-address" bajo el mapa para mostrar la direccion elegida.
        this._addressLabelId = containerId + '-address';
        this._showAddress(null);

        const defaultLat = lat || 40.4168;
        const defaultLng = lng || -3.7038;

        setTimeout(() => {
            this._map = L.map(containerId).setView([defaultLat, defaultLng], 10);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap',
                maxZoom: 19
            }).addTo(this._map);

            this._marker = L.marker([defaultLat, defaultLng], { draggable: true }).addTo(this._map);

            this._marker.on('dragend', (e) => {
                const pos = e.target.getLatLng();
                this.reverseGeocode(pos.lat, pos.lng);
            });

            this._map.on('click', (e) => {
                this._marker.setLatLng(e.latlng);
                this.reverseGeocode(e.latlng.lat, e.latlng.lng);
            });

            setTimeout(() => this._map.invalidateSize(), 200);
        }, 100);
    },

    searchLocation: function (query) {
        if (!query || query.trim().length < 3) return;

        this._pending = fetch(`https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&q=${encodeURIComponent(query)}&limit=1`, {
            headers: { 'Accept-Language': 'es' }
        })
            .then(r => r.json())
            .then(data => {
                if (data && data.length > 0) {
                    const result = data[0];
                    const lat = parseFloat(result.lat);
                    const lng = parseFloat(result.lon);

                    if (this._map && this._marker) {
                        this._map.setView([lat, lng], 16);
                        this._marker.setLatLng([lat, lng]);
                    }

                    this.extractAddress(result);
                }
            })
            .catch(err => console.error('Map search error:', err));
        return this._pending;
    },

    reverseGeocode: function (lat, lng) {
        this._showAddress('...');
        this._pending = fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1&accept-language=es`, {
            headers: { 'Accept-Language': 'es' }
        })
            .then(r => r.json())
            .then(data => {
                this.extractAddress(data);
            })
            .catch(err => console.error('Reverse geocode error:', err));
        return this._pending;
    },

    extractAddress: function (data) {
        if (!data) return;
        const addr = data.address || {};
        const country = addr.country || '';
        const city = addr.city || addr.town || addr.village || addr.municipality || addr.county || addr.state || '';

        const road = addr.road || addr.pedestrian || addr.street || '';
        const houseNumber = addr.house_number || '';
        let address = road && houseNumber ? `${road} ${houseNumber}` : (road || houseNumber);
        if (!address && data.display_name) {
            address = data.display_name.split(',')[0].trim();
        }
        if (address && addr.postcode) {
            address = `${address}, ${addr.postcode}`;
        }

        // Se guarda localmente en vez de empujarlo a .NET de inmediato: si el circuito de
        // Blazor Server se reconecto mientras el usuario buscaba, una llamada async hacia una
        // referencia .NET guardada quedaria apuntando a una instancia ya descartada. En su
        // lugar, el boton "Usar ubicacion" lo pide (getSelectedLocation) en el momento del
        // clic, cuando el circuito activo esta garantizado.
        this._selected = { country, city, address };
        this._showAddress([address, city, country].filter(Boolean).join(', '));
    },

    getSelectedLocation: async function () {
        // Sin busqueda ni clic todavia: usar la posicion actual del marcador.
        if (!this._selected && !this._pending && this._marker) {
            const pos = this._marker.getLatLng();
            this.reverseGeocode(pos.lat, pos.lng);
        }
        if (this._pending) {
            try { await this._pending; } catch (e) { /* ya logueado */ }
        }
        return this._selected;
    },

    _showAddress: function (text) {
        const label = this._addressLabelId && document.getElementById(this._addressLabelId);
        if (label) label.textContent = text || '';
    },

    closeMap: function () {
        if (this._map) {
            this._map.remove();
            this._map = null;
            this._marker = null;
        }
        this._selected = null;
        this._pending = null;
    }
};
