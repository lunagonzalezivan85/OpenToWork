// v2: the fetch handler used to cache ANY same-origin GET, including the
// dynamically-rendered page HTML (/, /register, /login, ...) which embeds
// per-session Blazor Server prerender/circuit markers. Serving that HTML from
// cache after a server restart/deploy desyncs the client from the new
// circuit and crashes it ("The list of component operations is not valid"),
// which then surfaces as "The POST request does not specify which form is
// being submitted" when a form on that stale page is submitted. Bumping the
// cache name also purges any already-cached bad entries from v1 installs.
const CACHE_NAME = 'opentowork-v2';
const ASSETS = [
  '/icon.svg',
  '/manifest.json',
  '/css/base.css',
  '/css/components.css',
  '/css/portal-nav.css',
  '/css/wizard-profile.css',
  '/css/responsive.css',
  '/themes/navy/theme.css'
];

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

  // Only cache-first the known static assets above. Page navigations (Razor
  // component HTML), the Blazor Server circuit/boot endpoints (_blazor,
  // _framework) and API calls must always go to the network uncached - see
  // the note on CACHE_NAME for why caching them corrupts the Blazor circuit.
  const url = new URL(event.request.url);
  const isStaticAsset = url.origin === self.location.origin &&
    (ASSETS.includes(url.pathname) || url.pathname.startsWith('/css/') || url.pathname.startsWith('/themes/'));
  if (!isStaticAsset) return;

  event.respondWith(
    caches.match(event.request).then((cached) => {
      return cached || fetch(event.request).then((response) => {
        if (response && response.status === 200) {
          const clone = response.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(event.request, clone));
        }
        return response;
      }).catch(() => cached);
    })
  );
});
