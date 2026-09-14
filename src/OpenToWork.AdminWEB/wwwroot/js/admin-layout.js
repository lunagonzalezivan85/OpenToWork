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
    toggleSubmenu: function (groupId) {
        const group = document.getElementById(groupId);
        if (!group) return;
        const expanded = group.classList.toggle('expanded');
        group.setAttribute('aria-expanded', expanded ? 'true' : 'false');
        localStorage.setItem('otwadmin-submenu-' + groupId, expanded ? '1' : '0');
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
    },
    initSubmenus: function () {
        document.querySelectorAll('.admin-nav-group--collapsible').forEach(function (g) {
            var id = g.id;
            if (!id) return;
            var saved = localStorage.getItem('otwadmin-submenu-' + id);
            if (saved === '0') {
                g.classList.remove('expanded');
                g.setAttribute('aria-expanded', 'false');
            } else {
                g.classList.add('expanded');
                g.setAttribute('aria-expanded', 'true');
            }
        });
    }
};

document.addEventListener('DOMContentLoaded', function () {
    window.adminLayout.init();
    window.adminLayout.initSubmenus();
});
