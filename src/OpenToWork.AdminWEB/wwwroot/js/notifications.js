window.notify = {
    error: function (message) {
        const container = document.getElementById('otw-notify-container') || createContainer();
        const toast = document.createElement('div');
        toast.className = 'otw-notify-toast otw-notify-toast--error';
        toast.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18" style="flex-shrink:0;"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg><span>' + message + '</span>';
        container.appendChild(toast);
        setTimeout(() => { toast.classList.add('otw-notify-toast--show'); }, 10);
        setTimeout(() => {
            toast.classList.remove('otw-notify-toast--show');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    },
    success: function (message) {
        const container = document.getElementById('otw-notify-container') || createContainer();
        const toast = document.createElement('div');
        toast.className = 'otw-notify-toast otw-notify-toast--success';
        toast.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18" style="flex-shrink:0;"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg><span>' + message + '</span>';
        container.appendChild(toast);
        setTimeout(() => { toast.classList.add('otw-notify-toast--show'); }, 10);
        setTimeout(() => {
            toast.classList.remove('otw-notify-toast--show');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    },
    warning: function (message) {
        const container = document.getElementById('otw-notify-container') || createContainer();
        const toast = document.createElement('div');
        toast.className = 'otw-notify-toast otw-notify-toast--warning';
        toast.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18" style="flex-shrink:0;"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg><span>' + message + '</span>';
        container.appendChild(toast);
        setTimeout(() => { toast.classList.add('otw-notify-toast--show'); }, 10);
        setTimeout(() => {
            toast.classList.remove('otw-notify-toast--show');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
};

window.swalSuccess = function (title, text, confirmText) {
    return Swal.fire({
        title: title,
        text: text,
        icon: 'success',
        confirmButtonText: confirmText || 'OK',
        confirmButtonColor: '#0066FF',
        customClass: { popup: 'otw-swal-popup' }
    });
};

window.swalError = function (title, text) {
    return Swal.fire({
        title: title,
        text: text,
        icon: 'error',
        confirmButtonText: 'OK',
        confirmButtonColor: '#0066FF',
        customClass: { popup: 'otw-swal-popup' }
    });
};

function createContainer() {
    const c = document.createElement('div');
    c.id = 'otw-notify-container';
    c.style.cssText = 'position:fixed;top:20px;right:20px;z-index:9999;display:flex;flex-direction:column;gap:8px;pointer-events:none;';
    document.body.appendChild(c);
    return c;
}
