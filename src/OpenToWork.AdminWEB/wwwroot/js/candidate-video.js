// Video de presentacion del candidato en su ficha (CandidateProfile.razor).
// Se descarga por /media/... (AdminWEB lo reenvia al AdminAPI) con el token que llega desde .NET,
// y se reproduce desde un object URL: el archivo no pasa por el circuito de Blazor.
window.candidateVideo = (function () {
    let currentUrl = null;

    return {
        load: async function (videoEl, userId, token) {
            try {
                const res = await fetch('/media/candidates/' + userId + '/video', {
                    headers: token ? { 'Authorization': 'Bearer ' + token } : {}
                });
                if (!res.ok) return false;
                if (currentUrl) URL.revokeObjectURL(currentUrl);
                currentUrl = URL.createObjectURL(await res.blob());
                videoEl.src = currentUrl;
                return true;
            } catch {
                return false;
            }
        },

        release: function () {
            if (currentUrl) URL.revokeObjectURL(currentUrl);
            currentUrl = null;
        }
    };
})();
