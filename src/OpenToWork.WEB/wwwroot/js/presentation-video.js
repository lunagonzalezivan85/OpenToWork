// Video de presentacion del candidato (pagina /presentation-video).
// Graba con la camara (MediaRecorder, corte automatico al minuto) o toma un archivo del movil,
// y lo sube directamente al API con XHR: el video no pasa por la memoria de WebAssembly.
// La URL base del API y el token llegan desde .NET en cada llamada; aqui no se guardan.
//
// Espacio en el servidor: todo se guarda a 480p y ~800 kbps (~5 MB por minuto).
// - Lo grabado ya sale asi de la camara.
// - Un archivo del movil (60-130 MB por minuto) se recomprime aqui, en el navegador, antes de subirlo:
//   se reproduce en un <video> oculto, se redibuja en un canvas de 480p y se graba con MediaRecorder.
//   Tarda lo que dura el video. Se hace al pulsar "Guardar" (gesto del usuario: permite el audio).
window.presentationVideo = (function () {
    const MAX_SECONDS = 60;
    // Margen para archivos del movil: un "1 minuto" grabado a mano suele pasar unos segundos.
    const MAX_FILE_SECONDS = 75;
    const MAX_BYTES = 20000000;               // PresentationVideoStorage.MaxBytes
    const MAX_SOURCE_BYTES = 500000000;       // archivo original antes de recomprimir
    const LONG_SIDE = 854, SHORT_SIDE = 480;  // 480p (horizontal o vertical)
    const VIDEO_BPS = 800000, AUDIO_BPS = 64000;

    let stream = null;            // camara + microfono
    let recorder = null;
    let chunks = [];
    let pending = null;           // Blob o File listo para subir
    let pendingCompressed = false;
    let pendingUrl = null;        // object URL del pendiente (para revisarlo)
    let currentUrl = null;        // object URL del video ya guardado
    let timer = null;

    function revoke(url) { if (url) URL.revokeObjectURL(url); }

    function pickMimeType() {
        const candidates = ['video/webm;codecs=vp9,opus', 'video/webm;codecs=vp8,opus', 'video/webm', 'video/mp4'];
        if (!window.MediaRecorder) return null;
        return candidates.find(t => MediaRecorder.isTypeSupported(t)) || '';
    }

    function recorderOptions() {
        const mimeType = pickMimeType();
        const opts = { videoBitsPerSecond: VIDEO_BPS, audioBitsPerSecond: AUDIO_BPS };
        if (mimeType) opts.mimeType = mimeType;
        return opts;
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

    // Tamaño de salida a 480p conservando la proporcion (tambien videos verticales del movil).
    function targetSize(w, h) {
        const landscape = w >= h;
        const scale = Math.min(1, (landscape ? LONG_SIDE : SHORT_SIDE) / w, (landscape ? SHORT_SIDE : LONG_SIDE) / h);
        const even = n => Math.max(2, Math.round(n * scale / 2) * 2);
        return { w: even(w), h: even(h) };
    }

    // Recomprime "file" a 480p / ~800 kbps. Devuelve un Blob, o null si este navegador no puede.
    async function compress(file, onProgress) {
        const canvas = document.createElement('canvas');
        if (!window.MediaRecorder || !canvas.captureStream) return null;

        const video = document.createElement('video');
        const url = URL.createObjectURL(file);
        video.src = url;
        video.playsInline = true;
        try {
            await new Promise((res, rej) => { video.onloadedmetadata = res; video.onerror = rej; });
        } catch {
            URL.revokeObjectURL(url);
            return null;   // no se puede decodificar aqui (p. ej. HEVC en Chrome)
        }

        const size = targetSize(video.videoWidth, video.videoHeight);
        canvas.width = size.w;
        canvas.height = size.h;
        const ctx2d = canvas.getContext('2d');
        const out = canvas.captureStream(25);

        // Audio: el elemento suena solo hacia la grabacion, no por los altavoces.
        let audioCtx = null;
        try {
            audioCtx = new (window.AudioContext || window.webkitAudioContext)();
            await audioCtx.resume();
            const dest = audioCtx.createMediaStreamDestination();
            audioCtx.createMediaElementSource(video).connect(dest);
            dest.stream.getAudioTracks().forEach(t => out.addTrack(t));
        } catch { /* sin audio capturable: se sube solo la imagen */ }

        const rec = new MediaRecorder(out, recorderOptions());
        const parts = [];
        rec.ondataavailable = e => { if (e.data && e.data.size > 0) parts.push(e.data); };

        const draw = () => ctx2d.drawImage(video, 0, 0, size.w, size.h);
        const drawTimer = setInterval(draw, 40);
        const progressTimer = setInterval(() => {
            if (video.duration) onProgress(Math.min(99, Math.round(video.currentTime * 100 / video.duration)));
        }, 500);

        try {
            draw();
            rec.start(1000);
            await video.play();
            await new Promise(res => { video.onended = res; });
        } catch {
            clearInterval(drawTimer); clearInterval(progressTimer);
            if (rec.state !== 'inactive') rec.stop();
            if (audioCtx) audioCtx.close();
            URL.revokeObjectURL(url);
            return null;
        }

        clearInterval(drawTimer);
        clearInterval(progressTimer);
        const stopped = new Promise(res => { rec.onstop = res; });
        rec.stop();
        await stopped;
        if (audioCtx) audioCtx.close();
        URL.revokeObjectURL(url);
        onProgress(100);
        return new Blob(parts, { type: rec.mimeType || 'video/webm' });
    }

    return {
        isRecordingSupported: function () {
            return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder && pickMimeType() !== null);
        },

        startCamera: async function (videoEl) {
            try {
                stream = await navigator.mediaDevices.getUserMedia({
                    video: { width: { ideal: LONG_SIDE }, height: { ideal: SHORT_SIDE }, frameRate: { ideal: 25 }, facingMode: 'user' },
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
            // 480p a ~800 kbps: un minuto ronda 5 MB.
            recorder = new MediaRecorder(stream, recorderOptions());
            recorder.ondataavailable = e => { if (e.data && e.data.size > 0) chunks.push(e.data); };
            recorder.onstop = () => {
                clearInterval(timer);
                pending = new Blob(chunks, { type: recorder.mimeType || 'video/webm' });
                pendingCompressed = true;
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
            if (file.size > MAX_SOURCE_BYTES) return { ok: false, error: 'size' };
            const seconds = await readDuration(file);
            if (seconds !== null && seconds > MAX_FILE_SECONDS) return { ok: false, error: 'duration', seconds: Math.round(seconds) };
            pending = file;
            pendingCompressed = false;
            showPending(videoEl);
            return { ok: true, seconds: seconds === null ? null : Math.round(seconds) };
        },

        // Paso previo a subir un archivo del movil. {ok, skipped, error, bytes}
        compressPending: async function (dotNetRef) {
            if (!pending) return { ok: false, error: 'none' };
            if (pendingCompressed) return { ok: true, skipped: true, bytes: pending.size };

            const blob = await compress(pending, p => dotNetRef.invokeMethodAsync('OnCompressProgress', p));
            if (blob && blob.size > 0 && blob.size < pending.size) {
                pending = blob;
            } else if (pending.size > MAX_BYTES) {
                // No se pudo recomprimir y el original no cabe.
                return { ok: false, error: blob ? 'size' : 'compress' };
            }
            pendingCompressed = true;
            return { ok: true, bytes: pending.size };
        },

        discardPending: function () {
            revoke(pendingUrl);
            pendingUrl = null;
            pending = null;
            pendingCompressed = false;
        },

        // XHR (no fetch) para poder informar del progreso de subida.
        uploadPending: function (baseUrl, token, dotNetRef) {
            return new Promise(resolve => {
                if (!pending) { resolve({ ok: false, status: 0 }); return; }
                if (pending.size > MAX_BYTES) { resolve({ ok: false, status: 413 }); return; }
                const form = new FormData();
                const ext = pending.type.includes('mp4') ? 'mp4' : pending.type.includes('quicktime') ? 'mov' : 'webm';
                form.append('file', pending, 'presentacion.' + ext);
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
