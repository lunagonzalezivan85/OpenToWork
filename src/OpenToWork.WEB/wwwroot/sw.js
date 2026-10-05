// v2: the fetch handler used to cache ANY same-origin GET, including the
// dynamically-rendered page HTML (/, /register, /login, ...) which embeds
// per-session Blazor Server prerender/circuit markers. Serving that HTML from
// cache after a server restart/deploy desyncs the client from the new
// circuit and crashes it ("The list of component operations is not valid"),
// which then surfaces as "The POST request does not specify which form is
// being submitted" when a form on that stale page is submitted. Bumping the
// cache name also purges any already-cached bad entries from v1 installs.
const CACHE_NAME = 'tratodirecto-v9';
const ASSETS = [
  '/icon.svg',
  '/manifest.json',
  '/css/base.css',
  '/css/components.css',
  '/css/bento-grid.css',
  '/css/portal-nav.css',
  '/css/home-v2.css',
  '/css/wizard-profile.css',
  '/css/profile-v2.css',
  '/css/responsive.css',
  '/themes/navy/theme.css'
];

// Recursos inmutables: SOLO las librerias vendoreadas de /lib/ (las versionamos a
// mano al actualizarlas). Los archivos de /_framework/ NO estan fingerprinteados en
// standalone WASM (los nombres son fijos: OpenToWork.WEB.dll, dotnet.js...), asi que
// cache-first serviria DLLs viejos tras cada deploy -> van por network-first abajo.
const IMMUTABLE_PATH = /^\/lib\//;

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => cache.addAll(ASSETS)).catch(() => {})
  );
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  if (event.request.method !== 'GET') return;

  const url = new URL(event.request.url);
  if (url.origin !== self.location.origin) return;

  // Inmutables (librerias vendoreadas versionadas): cache-first.
  if (IMMUTABLE_PATH.test(url.pathname)) {
    event.respondWith(
      caches.match(event.request).then((cached) => cached ||
        fetch(event.request).then((response) => {
          if (response && response.status === 200) {
            const clone = response.clone();
            caches.open(CACHE_NAME).then((cache) => cache.put(event.request, clone));
          }
          return response;
        })
      )
    );
    return;
  }

  // _framework (WASM runtime + DLLs) NO pasa por el SW: en dev la respuesta llega
  // vacia a traves de respondWith (module scripts son estrictos con el MIME) y sin
  // fingerprint cache-first serviria codigo viejo tras un deploy. Sin respondWith
  // el navegador negocia directo, que es lo correcto para el boot de Blazor.
  const isStaticAsset =
    ASSETS.includes(url.pathname) || url.pathname.startsWith('/css/')
    || url.pathname.startsWith('/themes/');
  if (!isStaticAsset) return;

  // Network-first: always fetch latest, fall back to cache only if offline
  event.respondWith(
    fetch(event.request).then((response) => {
      if (response && response.status === 200) {
        const clone = response.clone();
        caches.open(CACHE_NAME).then((cache) => cache.put(event.request, clone));
      }
      return response;
    }).catch(() => caches.match(event.request))
  );
});
