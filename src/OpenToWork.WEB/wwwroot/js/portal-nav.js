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

window.animateCounters = function () {
    document.querySelectorAll('.home-hero-stat-value[data-count]').forEach(function (el) {
        var target = parseInt(el.getAttribute('data-count'));
        var duration = 1500;
        var startTime = performance.now();
        function update(now) {
            var elapsed = now - startTime;
            var progress = Math.min(elapsed / duration, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            el.textContent = Math.floor(eased * target);
            if (progress < 1) requestAnimationFrame(update);
            else el.textContent = target;
        }
        requestAnimationFrame(update);
    });
};

window.startHeroVideo = function () {
    var v = document.getElementById('heroVideo');
    if (!v) return;
    v.muted = true;
    v.volume = 0;
    var playlist = ['/v01.mp4?v=2', '/v02.mp4?v=2'];
    var idx = 0;
    v.addEventListener('ended', function () {
        idx = (idx + 1) % playlist.length;
        v.src = playlist[idx];
        v.play().catch(function () {});
    });
    v.play().catch(function () {});
};

window.otAntiCheat = {
    start: function () {
        window.otAntiCheatFlags = 0;
        if (window.otAntiCheatHandler) document.removeEventListener('visibilitychange', window.otAntiCheatHandler);
        window.otAntiCheatHandler = function () { if (document.hidden) window.otAntiCheatFlags++; };
        document.addEventListener('visibilitychange', window.otAntiCheatHandler);
    },
    getFlags: function () {
        return window.otAntiCheatFlags || 0;
    }
};
