# Fase 7 — Busqueda de empleo con filtros y mapa (geo-busqueda)

**IA:** Iluna · **Fecha:** 2026-10-04 · **Rama:** `iluna-geo-search` · **Estado:** implementado, probado en local (API + radio + orden; mapa pendiente de revision visual).

El candidato filtra `/vacancies` por contrato, modalidad, experiencia, salario minimo y orden
(recientes, antiguas, mayor/menor salario, mas cercanas), y elige la ubicacion en un **mapa
vectorial** (Leaflet + MapLibre GL sobre OpenFreeMap) con **radio en km**. La geocodificacion
es Nominatim (OpenStreetMap), en cliente para el picker y en servidor para las vacantes.

## Modelo / migracion

`PT_Vacancies` gana `Latitude`/`Longitude` (double, nullable) y `GeocodedAt` (intentos de
geocodificacion; el backfill no reintenta ubicaciones no resolubles). Indice
`(Latitude, Longitude, Status, IsDeleted)` para el bounding box.

Migracion `VacancyGeoCoordinates` limpia (sin ruido de seeds, igual que HospitalityChallenges).

## Backend

- `GeocodingService` (`Core`): Nominatim con `User-Agent` identificable (politica OSM), cache
  `IMemoryCache` 24 h (tambien de fallos) y `Geocoding:*` configurable
  (`NominatimBaseUrl`, `CountryCodes`, `UserAgent`). Nunca rompe el guardado de una vacante.
- Geocodificacion automatica al **crear/editar** vacante (portal empresa + admin) y al
  convertir temporales, solo cuando cambia `Location`.
- `SearchPermanentVacancyDto` gana `SortBy` (`recent|oldest|salaryDesc|salaryAsc|distance`),
  `SalaryMax`, `Latitude`, `Longitude`, `RadiusKm`.
- `SearchVacanciesAsync`: con geo activo **ignora el filtro de texto Location** (la etiqueta
  de Nominatim no coincide con el texto libre de la vacante); bounding box en SQL + Haversine
  exacta en memoria (volumen bajo; si crece, migrar a `ST_Distance_Sphere` de MySQL). Devuelve
  `DistanceKm` por vacante. Sin geo, orden en SQL como siempre.
- Backfill: `POST api/admin/vacancies/geocode-missing?max=N` (solo SuperAdmin,
  `RequireStaffRole` vacio), ~1 req/s por la politica de Nominatim.
- `CreateVacancyDto.Title` ahora `[Required]` (tapaba un hueco: creaba vacantes sin titulo).
- Rate limit auth configurable: `RateLimiting:AuthPermitLimit` /
  `AdminAuthPermitLimit` (default 10/5 en produccion, 100 en Development para la suite de tests).

## Frontend (`OpenToWork.WEB`)

- Librerias vendoreadas en `wwwroot/lib` (sin CDN, igual que SweetAlert2): Leaflet 1.9.4,
  MapLibre GL 4.7.1 **variante CSP** (`maplibre-gl-csp.js` + worker mismo origen — la CSP de
  `web.config` no necesita `worker-src blob:`), `leaflet-maplibre-gl` 0.1.4 (bridge Leaflet↔MapLibre).
- `js/location-map.js`: `otwMap` (init/search/reverse/goTo/setRadius/picked/destroy), pin
  `divIcon` sin imagenes, circulo de radio, reverse geocoding a etiqueta corta
  (ciudad, region).
- `LocationMapPicker.razor` (Shared): modal con buscador Nominatim, click/drag del marcador,
  slider 5–100 km. Devuelve `LocationPickResult(lat,lng,radiusKm,label)`.
- `Vacancies.razor`: toolbar de filtros (contrato, modalidad, experiencia, salario min, orden)
  + boton Mapa; tags activos ampliables; estado en la URL (`?sort=&contract=&mode=&exp=&smin=&lat=&lng=&radius=`).
  Escribir a mano en el campo ubicacion desactiva el punto del mapa (vuelve a texto).
- CSP de `web.config`: `connect-src` suma `nominatim.openstreetmap.org` y
  `tiles.openfreemap.org`.
- i18n es/en en `vacancies.json` (`filterBar`, `contract`, `sort`, `map`).

## Como probar

1. `dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.API`.
2. Geocodificar existentes: `POST api/admin/vacancies/geocode-missing` (SuperAdmin) — en local
   se sembraron coords a mano para Madrid/Barcelona/Cartagena.
3. Portal → `/vacancies` → boton **Mapa** → buscar ciudad o clic → slider de radio → usar.
4. `GET api/permanentvacancies/search?Latitude=40.41&Longitude=-3.70&RadiusKm=600` →
   Madrid (0 km) + Barcelona (505 km), Cartagena fuera. `SortBy=salaryDesc` ordena.

## Anexo — pulido de arranque del portal (UX)

Reporte del usuario: al cargar el portal aparecian claves sin traducir (`common.*`),
la pantalla "Cargando Trato Directo…" tardaba ~2 s y los skeletons eran cajas planas.

- **Sin flash de claves**: `Routes.razor` no monta el `<Router>` hasta que
  `LanguageService.InitializeAsync()` termino; mientras tanto muestra el mismo splash
  del `index.html` (transicion invisible). Español por defecto salvo `opentowork-lang=en`.
- **Splash instantaneo**: `index.html` pinta un app-shell (nav + hero navy + cards
  skeleton) con CSS inline — primer paint <50 ms, sin texto "Cargando…".
- **Skeletons con forma**: `VacancySkeletonCard.razor` replica la silueta de
  `VacancyCard` (logo, titulo, meta, pie); usado en Home y `/vacancies`.
- **Revisitas casi instantaneas**: `sw.js` v8 sirve cache-first `/_framework/` y
  `/lib/` (fingerprinteados/vendoreados). `blazor.boot.json` y
  `blazor.webassembly.js` excluidos (no fingerprinteados) → el manifiesto siempre fresco.

## Pendiente / limites

- Vacantes sin `Location` resoluble quedan fuera de la busqueda por radio (aparecen en la
  busqueda por texto). El backfill marca `GeocodedAt` para no reintentar.
- Radio maximo 1000 km; picker 100 km (slider).
- Tiles OpenFreeMap y Nominatim salen del navegador del candidato (ver
  `docs/COOKIES_Y_PRIVACIDAD.md` §3.1); si el volumen crece, autoalojar tiles
  (Protomaps/pmtiles) y un Nominatim propio — `Geocoding:NominatimBaseUrl` ya lo permite.
- El mapa del admin (alta de empresas) sigue usando Leaflet raster por `unpkg.com`;
  convendria vendorearlo igual en una fase posterior.
