window.adminLayout = {
    // Movil (<=768px): panel off-canvas con overlay, como antes.
    // Escritorio (>768px): colapsa/expande el panel de forma persistente
    // (se recuerda en localStorage), sin overlay.
    toggleSidebar: function () {
        const sidebar = document.getElementById('adminSidebar');
        const main = document.getElementById('adminMain');

        if (window.innerWidth <= 768) {
            const overlay = document.getElementById('adminSidebarOverlay');
            if (sidebar && overlay) {
                sidebar.classList.toggle('open');
                overlay.classList.toggle('active');
            }
            return;
        }

        if (sidebar && main) {
            const collapsed = sidebar.classList.toggle('collapsed');
            main.classList.toggle('sidebar-collapsed', collapsed);
            localStorage.setItem('otwadmin-sidebar-collapsed', collapsed ? '1' : '0');
        }
    },
    // Solo cierra el off-canvas movil (clic en el overlay). No toca el
    // colapso de escritorio, que es una preferencia persistente del usuario.
    closeSidebar: function () {
        const sidebar = document.getElementById('adminSidebar');
        const overlay = document.getElementById('adminSidebarOverlay');
        if (sidebar) sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('active');
    },
    init: function () {
        if (window.innerWidth <= 768) return;
        if (localStorage.getItem('otwadmin-sidebar-collapsed') !== '1') return;

        const sidebar = document.getElementById('adminSidebar');
        const main = document.getElementById('adminMain');
        if (sidebar && main) {
            sidebar.classList.add('collapsed');
            main.classList.add('sidebar-collapsed');
        }
    }
};

document.addEventListener('DOMContentLoaded', function () {
    window.adminLayout.init();
});
