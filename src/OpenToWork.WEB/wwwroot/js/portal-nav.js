window.notifSidebar = {
    toggle: function () {
        const sidebar = document.getElementById('notifSidebar');
        const overlay = document.getElementById('notifOverlay');
        if (sidebar && overlay) {
            sidebar.classList.toggle('open');
            overlay.classList.toggle('active');
        }
    },
    close: function () {
        const sidebar = document.getElementById('notifSidebar');
        const overlay = document.getElementById('notifOverlay');
        if (sidebar) sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('active');
    }
};
