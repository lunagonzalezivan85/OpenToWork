window.themeSwitcher = {
    set: function (theme) {
        const link = document.getElementById('theme-stylesheet');
        if (link) {
            // Cache-bust: sin esto el navegador puede servir un theme.css viejo
            // en cache indefinidamente (el link no tenia query string), asi que
            // ediciones a las variables de tema no se veian sin hard-refresh.
            link.href = '/themes/' + theme + '/theme.css?v=' + Date.now();
        }
        localStorage.setItem('opentowork-theme', theme);
    },
    getSaved: function () {
        return localStorage.getItem('opentowork-theme') || 'navy';
    },
    init: function () {
        const saved = this.getSaved();
        this.set(saved);
    }
};

document.addEventListener('DOMContentLoaded', function () {
    window.themeSwitcher.init();
});
