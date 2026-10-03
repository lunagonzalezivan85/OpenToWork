// Video de presentacion del candidato (pagina /presentation-video).
// Graba con la camara (MediaRecorder, corte automatico al minuto) o toma un archivo del movil,
// y lo sube directamente al API con fetch/XHR: el video no pasa por la memoria de WebAssembly.
// La URL base del API y el token llegan desde .NET en cada llamada; aqui no se guardan.
window.presentationVideo = (function () {
    const MAX_SECONDS = 60;
    // Margen para archivos del movil: un "1 minuto" grabado a mano suele pasar unos segundos.
    const MAX_FILE_SECONDS = 75;
    const MAX_BYTES = 95000000; // PresentationVideoStorage.MaxBytes

    let stream = null;      // camara + microfono
    let recorder = null;
    let chunks = [];
    let pending = null;     // Blob o File listo para subir
    let pendingUrl = null;  // object URL del pendiente (para revisarlo)
    let currentUrl = null;  // object URL del video ya guardado
    let timer = null;

    function revoke(url) { if (url) URL.revokeObjectURL(url); }

    function pickMimeType() {
        const candidates = ['video/webm;codecs=vp9,opus', 'video/webm;codecs=vp8,opus', 'video/webm', 'video/mp4'];
        if (!window.MediaRecorder) return null;
        return candidates.find(t => MediaRecorder.isTypeSupported(t)) || '';
    }

    function showPending(videoEl) {
        revoke(pendingUrl);
        pendingUrl = URL.createObjectURL(pending);
        videoEl.srcObject = null;
        videoEl.muted = false;
        videoEl.controls = true;
        videoEl.src = pendingUrl;
    }

    function readDuration(file) {
        return new Promise(resolve => {
            const probe = document.createElement('video');
            const url = URL.createObjectURL(file);
            const done = (seconds) => { URL.revokeObjectURL(url); resolve(seconds); };
            probe.preload = 'metadata';
            probe.onloadedmetadata = () => done(isFinite(probe.duration) ? probe.duration : null);
            // Algunos formatos (p. ej. HEVC de iPhone en Chrome) no se pueden leer aqui: no se bloquea.
            probe.onerror = () => done(null);
            probe.src = url;
        });
    }

    return {
        isRecordingSupported: function () {
            return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder && pickMimeType() !== null);
        },

        startCamera: async function (videoEl) {
            try {
                stream = await navigator.mediaDevices.getUserMedia({
                    video: { width: { ideal: 1280 }, height: { ideal: 720 }, facingMode: 'user' },
                    audio: true
                });
                videoEl.srcObject = stream;
                videoEl.muted = true;      // sin eco mientras se graba
                videoEl.controls = false;
                await videoEl.play();
                return { ok: true };
            } catch (e) {
                return { ok: false, error: e && e.name === 'NotAllowedError' ? 'denied' : 'unavailable' };
            }
        },

        startRecording: function (dotNetRef) {
            if (!stream) return false;
            chunks = [];
            const mimeType = pickMimeType();
            // ~2,5 Mbps: un minuto ronda 19 MB, muy por debajo del limite.
            recorder = new MediaRecorder(stream, mimeType ? { mimeType, videoBitsPerSecond: 2500000 } : undefined);
            recorder.ondataavailable = e => { if (e.data && e.data.size > 0) chunks.push(e.data); };
            recorder.onstop = () => {
                clearInterval(timer);
                pending = new Blob(chunks, { type: recorder.mimeType || 'video/webm' });
                dotNetRef.invokeMethodAsync('OnRecordingStopped', pending.size);
            };
            recorder.start(1000);
            let seconds = 0;
            timer = setInterval(() => {
                seconds++;
                dotNetRef.invokeMethodAsync('OnRecordingTick', seconds);
                if (seconds >= MAX_SECONDS && recorder.state === 'recording') recorder.stop();
            }, 1000);
            return true;
        },

        stopRecording: function () {
            if (recorder && recorder.state === 'recording') recorder.stop();
        },

        // Tras grabar: apaga la camara y muestra lo grabado para revisarlo.
        reviewRecording: function (videoEl) {
            this.stopCamera();
            if (pending) showPending(videoEl);
        },

        stopCamera: function () {
            if (stream) stream.getTracks().forEach(t => t.stop());
            stream = null;
        },

        pickFile: async function (inputEl, videoEl) {
            const file = inputEl.files && inputEl.files[0];
            inputEl.value = '';
            if (!file) return { ok: false, error: 'none' };
            if (!file.type.startsWith('video/')) return { ok: false, error: 'type' };
            if (file.size > MAX_BYTES) return { ok: false, error: 'size' };
            const seconds = await readDuration(file);
            if (seconds !== null && seconds > MAX_FILE_SECONDS) return { ok: false, error: 'duration', seconds: Math.round(seconds) };
            pending = file;
            showPending(videoEl);
            return { ok: true, seconds: seconds === null ? null : Math.round(seconds) };
        },

        discardPending: function () {
            revoke(pendingUrl);
            pendingUrl = null;
            pending = null;
        },

        // XHR (no fetch) para poder informar del progreso de subida.
        uploadPending: function (baseUrl, token, dotNetRef) {
            return new Promise(resolve => {
                if (!pending) { resolve({ ok: false, status: 0 }); return; }
                const form = new FormData();
                const ext = pending.type.includes('mp4') ? 'mp4' : pending.type.includes('quicktime') ? 'mov' : 'webm';
                form.append('file', pending, pending.name || ('presentacion.' + ext));
                const xhr = new XMLHttpRequest();
                xhr.open('POST', baseUrl + 'api/profile/video');
                if (token) xhr.setRequestHeader('Authorization', 'Bearer ' + token);
                xhr.upload.onprogress = e => {
                    if (e.lengthComputable) dotNetRef.invokeMethodAsync('OnUploadProgress', Math.round(e.loaded * 100 / e.total));
                };
                xhr.onload = () => {
                    let message = null;
                    try { message = JSON.parse(xhr.responseText).message || null; } catch { }
                    resolve({ ok: xhr.status >= 200 && xhr.status < 300, status: xhr.status, message });
                };
                xhr.onerror = () => resolve({ ok: false, status: 0 });
                xhr.send(form);
            });
        },

        // Carga el video guardado (con el token) y lo pone en el reproductor.
        loadCurrent: async function (baseUrl, token, videoEl) {
            try {
                const res = await fetch(baseUrl + 'api/profile/video', { headers: token ? { 'Authorization': 'Bearer ' + token } : {} });
                if (!res.ok) return false;
                revoke(currentUrl);
                currentUrl = URL.createObjectURL(await res.blob());
                videoEl.srcObject = null;
                videoEl.muted = false;
                videoEl.controls = true;
                videoEl.src = currentUrl;
                return true;
            } catch {
                return false;
            }
        },

        dispose: function () {
            clearInterval(timer);
            this.stopCamera();
            this.discardPending();
            revoke(currentUrl);
            currentUrl = null;
        }
    };
})();
