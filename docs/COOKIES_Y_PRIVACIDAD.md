# Cookies y almacenamiento en el navegador — Politica y plan de trabajo

> Autor: Dsiezar (con Claude), 27-Sep-2026. Estado: **propuesta, pendiente de decisiones de Darwin y de revision legal.**
> Alcance: portal publico (`tratodirecto.es`, `OpenToWork.WEB`) y panel administrativo (`admin.tratodirecto.es`, `OpenToWork.AdminWEB`).

---

## 1. Resumen

- Hoy **ninguna de las dos webs usa cookies de analitica ni de publicidad**. Todo lo que se guarda en el navegador es tecnico (sesion, seguridad) o de preferencia (idioma, tema).
- Con eso, la ley espanola **no exige banner de consentimiento**, pero **si exige informar**: falta una pagina de Politica de Cookies.
- El unico punto debil es que el portal carga SweetAlert2 desde un CDN externo (`cdn.jsdelivr.net`): no pone cookies, pero cada visita le entrega la IP del usuario a un tercero. Se propone servirlo desde el propio sitio (ver seccion 5).
- Solo haria falta un banner si en el futuro se agrega analitica con cookies (Google Analytics), pixeles de redes o contenido embebido de terceros (YouTube, mapas de Google). Para ese caso queda disenada la Fase C5.

---

## 2. Marco legal aplicable (Espana / UE)

| Norma | Que exige |
|---|---|
| **LSSI, art. 22.2** | Para guardar o leer informacion en el equipo del usuario hace falta consentimiento informado, **salvo** lo estrictamente necesario para prestar el servicio que el usuario pidio o para transmitir la comunicacion. Aplica a cookies **y** a localStorage, sessionStorage, IndexedDB y caches de service worker. |
| **Guia sobre el uso de cookies de la AEPD (actualizada 2023)** | Exentas de consentimiento: cookies tecnicas, de sesion, de seguridad y de preferencias elegidas por el usuario (idioma, tema). Aunque esten exentas, **hay que informar** de ellas. Si hay banner: "Rechazar" en la primera capa con la misma visibilidad que "Aceptar", nada premarcado, retirar el consentimiento tan facil como darlo, sin muros de cookies sin alternativa, y renovar el consentimiento como maximo cada 24 meses. |
| **RGPD, art. 6, 7 y 13** | La IP es un dato personal: enviarla a un tercero (CDN) es una comunicacion de datos que debe tener base legal e informarse. Si hay consentimiento, hay que poder **demostrarlo** (art. 7.1). |

> El texto final de la politica debe revisarlo un **asesor legal**, igual que `/privacy` y `/terms`.

---

## 3. Inventario actual (verificado el 27-Sep-2026 en el codigo y en produccion)

### 3.1 Portal — `tratodirecto.es`

En produccion el portal **no crea ninguna cookie** (verificado con `curl`: sin `Set-Cookie`). Todo va en `localStorage`:

| Nombre | Donde | Tipo | Finalidad | Duracion | Titular |
|---|---|---|---|---|---|
| `opentowork-token` | localStorage | Tecnica | Token de acceso de la sesion iniciada | Hasta cerrar sesion | Trato Directo |
| `opentowork-refresh-token` | localStorage | Tecnica | Renovar la sesion sin volver a pedir la contrasena | Hasta cerrar sesion | Trato Directo |
| `opentowork-user-id` | localStorage | Tecnica | Identificar al usuario conectado | Hasta cerrar sesion | Trato Directo |
| `opentowork-role` | localStorage | Tecnica | Saber si la cuenta es de candidato o de empresa (menu) | Hasta cerrar sesion | Trato Directo |
| `opentowork-lang` | localStorage | Preferencia | Recordar el idioma elegido | Permanente, hasta borrarlo | Trato Directo |
| `opentowork-theme` | localStorage | Preferencia | Recordar el tema visual elegido | Permanente, hasta borrarlo | Trato Directo |
| `tratodirecto-v7` | Cache del service worker | Tecnica | Guardar estilos e iconos para que el portal cargue mas rapido | Hasta la siguiente version del portal | Trato Directo |

Terceros que recibe el navegador del visitante:

| Tercero | Que se carga | Cookies | Datos que recibe |
|---|---|---|---|
| `cdn.jsdelivr.net` | SweetAlert2 (JS + CSS), en `wwwroot/index.html` | No | IP, navegador, pagina de origen (Referer) |
| Cloudflare | Proxy / tunel delante de todo el sitio | No en el portal | Todo el trafico (es nuestro encargado de tratamiento) |

### 3.2 Panel administrativo — `admin.tratodirecto.es`

Uso interno (solo personal de TD).

| Nombre | Tipo | Finalidad | Duracion | Titular |
|---|---|---|---|---|
| `CF_AppSession` (y `CF_Authorization` tras entrar) | Tecnica / seguridad | Control de acceso de Cloudflare Access | 1 dia | Cloudflare (por cuenta de TD) |
| `.AspNetCore.Antiforgery.*` | Tecnica / seguridad | Proteger formularios contra falsificacion de peticiones | Sesion | Trato Directo |
| Token y preferencias en localStorage | Tecnica / preferencia | Sesion del admin, idioma, tema | Hasta cerrar sesion / permanente | Trato Directo |

Terceros:

| Tercero | Que se carga | Uso |
|---|---|---|
| `unpkg.com` | Leaflet 1.9.4 (JS + CSS) | Mapa del alta de empresas |
| `cdn.jsdelivr.net` | SweetAlert2 | Dialogos |
| `tile.openstreetmap.org` | Teselas del mapa | Mapa |
| `nominatim.openstreetmap.org` | Buscar direccion / direccion desde el mapa | Mapa |

---

## 4. Estado actual de los textos legales

- `/privacy`, punto 11 "Almacenamiento en tu navegador": ya dice que solo se guarda lo tecnico (sesion, idioma, tema), que no hay cookies publicitarias y que se pedira consentimiento si se agregan cookies analiticas o de terceros. **Es correcto pero incompleto**: no hay tabla, no menciona el service worker ni los terceros (CDN, Cloudflare).
- No existe pagina `/cookies`.
- El pie del portal enlaza Privacidad y Terminos, no Cookies.

---

## 5. Que pasa si se quita `cdn.jsdelivr.net` del portal

**Uso real:** SweetAlert2 solo se usa en 2 pantallas **del portal de empresa**: `MyVacancies.razor` (`/my-vacancies`, 6 avisos) y `VacancyManage.razor` (`/my-vacancies/{id}`, 5 avisos: exito, error y la confirmacion de "Eliminar vacante"). Los candidatos y visitantes nunca lo usan, pero hoy lo descargan todos en cada visita porque esta en `index.html`.

**Si se borra sin reemplazo:** esas llamadas `JS.InvokeVoidAsync("Swal.fire", ...)` fallan (`Swal is not defined`). Al crear, publicar, cerrar o eliminar una vacante la empresa no veria el aviso, y la accion que va despues del aviso podria no ejecutarse. **No se puede quitar a secas.**

**Lo que se propone: autoalojarlo** (misma libreria, servida desde `tratodirecto.es`):

1. Descargar `sweetalert2.all.min.js` y `sweetalert2.min.css` de una version **fija** (hoy se pide `@11`, que resuelve sola a la ultima 11.x) a `wwwroot/lib/sweetalert2/`.
2. Cambiar las 2 lineas de `wwwroot/index.html` a rutas locales.
3. Quitar `https://cdn.jsdelivr.net` de la CSP de `src/OpenToWork.WEB/web.config` (`script-src` y `style-src`).

**Que se gana:**

| | Hoy (jsdelivr) | Autoalojado |
|---|---|---|
| Datos a terceros | IP de cada visitante a jsdelivr | Ninguno |
| Politica de cookies | Hay que declarar el tercero | El portal queda 100 % propio |
| Seguridad | Se ejecuta lo que jsdelivr sirva en `@11`, sin control de version ni SRI; la CSP tiene que confiar en todo jsdelivr | Version fija, revisada, dentro del repo; CSP mas estricta (`script-src 'self'`) |
| Disponibilidad | Si jsdelivr cae o esta bloqueado (redes de empresa), los avisos fallan | Depende solo de nuestro servidor |
| Coste | — | ~80 KB mas en el repo; actualizar a mano cuando haya version nueva |

**Riesgo del cambio:** bajo. Es la misma libreria y la misma API. El service worker no cachea SweetAlert2, asi que no hay cache vieja que invalidar.

---

## 6. Hallazgos encontrados al revisar (fuera del alcance de cookies)

1. **Probable bug — "Eliminar vacante" en el portal de empresa** (`VacancyManage.razor:360`): se hace `JS.InvokeAsync<bool>("Swal.fire", ...)`, pero `Swal.fire` devuelve un objeto (`{ isConfirmed, isDismissed, ... }`), no un `bool`. La conversion deberia lanzar una excepcion al confirmar y la vacante no se eliminaria. **Sin verificar en navegador.** Arreglo: leer `isConfirmed` desde un pequeno helper JS, o usar un `record` con `IsConfirmed`.
2. **Probable bug — busqueda de direccion del mapa en el admin**: la CSP que se agrego en la revision de seguridad del 25-Sep (`fa6405f`, `OpenToWork.AdminWEB/Program.cs`) tiene `connect-src 'self' ws: wss:`, y `map-picker.js` hace `fetch` a `nominatim.openstreetmap.org`. El navegador deberia bloquearlo: el mapa se ve (las teselas entran por `img-src https:`), pero buscar una direccion o tomar la direccion del punto elegido no funcionaria. **Sin verificar en navegador.** Arreglo: agregar `https://nominatim.openstreetmap.org` a `connect-src`.

---

## 7. Fases de trabajo

Cada fase es independiente y se puede desplegar sola. Las fases C1 a C4 no necesitan banner. C5 y C6 solo se hacen si se decide agregar analitica o contenido de terceros.

### Fase C1 — Portal sin terceros (autoalojar SweetAlert2) — HECHA (27-Sep)

- [x] Descargar SweetAlert2 en version fija (11.26.25, del registro de npm con checksum verificado) a `src/OpenToWork.WEB/wwwroot/lib/sweetalert2/`.
- [x] `wwwroot/index.html`: rutas locales en lugar de jsdelivr.
- [x] `web.config`: quitar `https://cdn.jsdelivr.net` de `script-src` y `style-src`.
- [x] Arreglar de paso el hallazgo 6.1 (`InvokeAsync<bool>` en "Eliminar vacante").

**Criterio de cierre:** en `/my-vacancies` y `/my-vacancies/{id}` los avisos y la confirmacion de eliminar funcionan; la pestana Red del navegador no muestra ninguna peticion fuera de `tratodirecto.es`; la consola no muestra errores de CSP.
**Esfuerzo:** pequeno (1 commit).

### Fase C2 — Pagina `/cookies` y enlaces

- [ ] Pagina nueva `Cookies.razor` (`/cookies`) en el portal, con el mismo formato que `Privacy.razor` y `Terms.razor`. Texto base en la seccion 8 de este documento.
- [ ] Claves i18n `common.cookies.*` en `es` y `en`.
- [ ] Enlace "Cookies" en el pie del portal, junto a Privacidad y Terminos.
- [ ] `/privacy` punto 11: remitir a `/cookies` para el detalle.
- [ ] Registro (`/register`): mencionar la Politica de Cookies junto a la aceptacion de terminos.

**Criterio de cierre:** `/cookies` accesible sin sesion, en espanol e ingles, enlazada desde pie, privacidad y registro; la tabla coincide con el inventario de la seccion 3.1 despues de C1.
**Esfuerzo:** pequeno-medio (1 commit).

### Fase C3 — Panel administrativo

- [ ] Autoalojar Leaflet 1.9.4 y SweetAlert2 en `src/OpenToWork.AdminWEB/wwwroot/lib/` y quitar `unpkg.com` y `cdn.jsdelivr.net` de la CSP (`Program.cs`).
- [ ] Arreglar el hallazgo 6.2: agregar `https://nominatim.openstreetmap.org` a `connect-src`.
- [ ] Los mapas de OpenStreetMap siguen siendo externos (no hay alternativa razonable autoalojada); se declaran en la nota interna.
- [ ] Nota breve para el personal (en la pantalla de login del admin o en el documento interno de uso) sobre las cookies tecnicas del panel. Sin banner: es una herramienta de trabajo interna.

**Criterio de cierre:** el alta de empresa con mapa busca direcciones y devuelve la direccion del punto; ninguna peticion a `unpkg`/`jsdelivr`.
**Esfuerzo:** pequeno (1 commit). **Opcional** — el admin es interno.

### Fase C4 — Revision legal y vigencia

- [ ] Enviar `/cookies`, `/privacy` y `/terms` al asesor legal.
- [ ] Incorporar correcciones y poner "Ultima actualizacion: fecha" en las tres paginas.
- [ ] Acordar quien revisa el inventario (seccion 3) cada vez que se agregue un script, CDN o servicio externo. Propuesta: regla en el README, "ningun script de terceros sin actualizar `docs/COOKIES_Y_PRIVACIDAD.md` y `/cookies`".

**Criterio de cierre:** textos aprobados por el asesor y publicados.

### Fase C5 — Analitica (condicional: solo si se decide medir visitas)

**Opcion A (recomendada): analitica sin cookies.** Cloudflare Web Analytics (ya estamos detras de Cloudflare; no guarda nada en el navegador) o Plausible autoalojado.
- [ ] Activarla y agregarla a la CSP.
- [ ] Mencionarla en `/cookies` y `/privacy` (tercero, datos agregados, sin cookies).
- **Sigue sin hacer falta banner.**

**Opcion B: Google Analytics, pixeles de redes o videos embebidos.** Requiere gestor de consentimiento propio (componente Blazor, sin servicio de pago):
- [ ] Banner de primera capa: **Aceptar todo / Rechazar todo / Configurar**, los tres con el mismo peso visual; enlace a `/cookies`.
- [ ] Panel "Configurar": Necesarias (siempre activas, sin interruptor), Analiticas y Marketing (desactivadas por defecto).
- [ ] Ningun script de terceros se carga antes de aceptar su categoria (cargador JS que lee el consentimiento).
- [ ] Guardar la decision en `localStorage` como `td-cookie-consent` = `{ version, fecha, analiticas, marketing }`.
- [ ] Enlace permanente "Configurar cookies" en el pie para cambiar o retirar la decision.
- [ ] Volver a preguntar si cambia la `version` de la politica o a los 24 meses.
- [ ] Sin muro de cookies: el portal funciona igual si se rechaza.

**Criterio de cierre (B):** con "Rechazar todo", la pestana Red no muestra ninguna peticion a los terceros de analitica/marketing; con "Aceptar", si; el enlace del pie reabre el panel.

### Fase C6 — Registro de consentimientos en servidor (condicional: solo con C5-B)

- [ ] Tabla `PT_CookieConsents` (Id, identificador anonimo del navegador, version de la politica, categorias, fecha, IP truncada/hash, UserId si hay sesion) y endpoint `POST api/consent`.
- [ ] Sirve para **demostrar** el consentimiento ante una reclamacion (RGPD art. 7.1).
- [ ] Recordatorio: requiere migracion, que el despliegue aplica automaticamente desde el 27-Sep (`eb12cba`).

---

## 8. Texto base para la pagina `/cookies` (borrador para el asesor legal)

> **Politica de Cookies**
>
> **Que son.** Las cookies y el almacenamiento local son pequenos archivos o datos que una web guarda en tu navegador para funcionar o recordar tus preferencias.
>
> **Que usamos.** Trato Directo solo usa almacenamiento **tecnico y de preferencias**, necesario para prestarte el servicio que pides. **No usamos cookies de analitica, de publicidad ni de redes sociales**, y no compartimos esta informacion con terceros para esos fines. Por eso no te pedimos consentimiento: la ley lo exime en este caso (art. 22.2 LSSI), pero te informamos de todo lo que guardamos.
>
> *[Tabla de la seccion 3.1]*
>
> **Proveedores.** Nuestro sitio se sirve a traves de Cloudflare, que actua como encargado del tratamiento para entregar la web de forma segura.
>
> **Como borrarlo.** Al cerrar sesion se borran los datos de sesion. Puedes borrar todo desde la configuracion de tu navegador (datos de sitios / almacenamiento). Si lo haces, tendras que volver a iniciar sesion y elegir idioma y tema.
>
> **Cambios.** Si algun dia incorporamos cookies que requieran tu consentimiento, te lo pediremos antes de usarlas y actualizaremos esta pagina.
>
> Ultima actualizacion: *[fecha]*. Responsable: *[razon social y correo de privacidad, desde Datos de la Empresa]*.

---

## 9. Decisiones pendientes (Darwin)

1. ¿Se aprueba el orden **C1 → C2 → C4**, con C3 opcional?
2. ¿Se va a medir visitas? Si si: ¿**Opcion A** (sin cookies, sin banner) o **Opcion B** (Google Analytics, con banner)?
3. ¿El panel admin entra en el alcance (Fase C3) o se deja como esta por ser interno?
4. ¿Quien es el asesor legal y cuando se le envian los textos?
