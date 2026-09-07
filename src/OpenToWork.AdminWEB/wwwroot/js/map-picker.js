window.mapPicker = {
    _map: null,
    _marker: null,
    _dotNetRef: null,

    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
    },

    openMap: function (containerId, lat, lng) {
        const el = document.getElementById(containerId);
        if (!el) return;

        if (this._map) {
            this._map.remove();
            this._map = null;
            this._marker = null;
        }

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

        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=1`, {
            headers: { 'Accept-Language': 'es' }
        })
            .then(r => r.json())
            .then(data => {
                if (data && data.length > 0) {
                    const result = data[0];
                    const lat = parseFloat(result.lat);
                    const lng = parseFloat(result.lon);

                    if (this._map && this._marker) {
                        this._map.setView([lat, lng], 12);
                        this._marker.setLatLng([lat, lng]);
                    }

                    this.extractAddress(result);
                }
            })
            .catch(err => console.error('Map search error:', err));
    },

    reverseGeocode: function (lat, lng) {
        fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=10&accept-language=es`, {
            headers: { 'Accept-Language': 'es' }
        })
            .then(r => r.json())
            .then(data => {
                this.extractAddress(data);
            })
            .catch(err => console.error('Reverse geocode error:', err));
    },

    extractAddress: function (data) {
        if (!data) return;
        const addr = data.address || {};
        const country = addr.country || '';
        const city = addr.city || addr.town || addr.village || addr.municipality || addr.county || addr.state || '';

        if (this._dotNetRef) {
            this._dotNetRef.invokeMethodAsync('OnLocationSelected', country, city);
        }
    },

    closeMap: function () {
        if (this._map) {
            this._map.remove();
            this._map = null;
            this._marker = null;
        }
    }
};
