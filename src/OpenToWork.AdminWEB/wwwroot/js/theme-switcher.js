window.themeSwitcher = {
    set: function (theme) {
        const link = document.getElementById('theme-stylesheet');
        if (link) {
            link.href = '/themes/' + theme + '/theme.css';
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
