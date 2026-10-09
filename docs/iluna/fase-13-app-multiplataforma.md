# Fase 13 — App multiplataforma (Ionic React + Capacitor)

Rama `iluna-app-multiplataforma` (apilada sobre `iluna-sec-rgpd`).

## Alcance de esta entrega

Base de la app en `app-multiplataforma/` — una base de código para web, Android e iOS
contra la misma `OpenToWork.API`.

- **Stack:** Ionic React 9 + React 19 + Vite + Capacitor 8 (`capacitor.config.ts`,
  appId `es.tratodirecto.app`, webDir `dist`).
- **Tema:** paleta del design system portada a variables Ionic (`theme/variables.css`):
  primary `#0066FF`, fondo `#F0F7FF`, texto `#0B132B`, dark "navy".
- **Sesión:** misma política que el portal (H-04) — access token en memoria, refresh en
  Capacitor Preferences, renovación silenciosa al abrir la app, `revoke` al cerrar sesión
  y `revoke-all` en Perfil.
- **Roles:** `PrimaryRole` 0 = candidato (Vacantes/Postulaciones/Mensajes/Perfil),
  1 = empresa (Mis vacantes/Buscar candidatos/Mensajes/Perfil).

## Pantallas funcionales

| Pantalla | Endpoint usado |
|---|---|
| Login | `POST /api/auth/login` (+ refresh/revoke/revoke-all) |
| Vacantes (candidato) | `GET /api/vacancies/search` público + `POST /api/applications` |
| Mis postulaciones | `GET /api/applications/my` |
| Mis vacantes (empresa) | `GET /api/permanentvacancies/my-company` |
| Buscar candidatos | `GET /api/candidates/search` + `POST {id}/request` (403 si no verificada — se muestra el aviso) |
| Mensajes | `GET /api/messages/conversations` (lista; hilo pendiente) |
| Panel | `applications/my` o `permanentvacancies/my-company` (resumen) |
| Mi proceso (candidato) | `GET /api/candidates/me/process` (etapas + entregas) |
| Entregados (empresa) | `GET /api/deliveries/my` |
| Noticias | `GET /api/news` |

## Navegación

Barra inferior con las secciones esenciales del rol + **menú lateral (hamburguesa) con el
menú completo** — el mismo set que el portal: Panel, Vacantes/Mis vacantes,
Postulaciones/Buscar candidatos, Mi proceso/Entregados, Mensajes, Noticias, Perfil,
Cerrar sesión.

**Trampa de Ionic aprendida:** los `IonTabButton` deben ser hijos directos de
`IonTabBar` — envolverlos en un `<>` fragment los deja fuera del mapa de tabs y no se
renderizan (por eso solo se veían Mensajes y Perfil).

## Verificación

- `npm run build` OK (tsc + vite, dist generado).
- Dev en `:5150` + Playwright: login real `qa.candidato@test.dev` → lista de vacantes
  reales de la API con botón Postularme; navegación por tabs OK.
- CORS dev: añadido `http://localhost:5150` a `appsettings.Development.json`.

## Notas / pendientes

- **npm audit:** 12 avisos, todos en dev-deps del template (cypress/extract-zip,
  vitest mocker) y `react-router@6` fijado por `@ionic/react-router` 9 — las
  "correcciones" de audit serían breaking; revisar cuando suba Ionic.
- Roadmap funcional completo en `app-multiplataforma/README.md` (registro, detalle de
  vacante, wizard de perfil, hilo de mensajes, entregas, push, geo, i18n…).
- Las plataformas nativas (`android/`/`ios/`) se generan con `npx cap add` cuando haya
  SDK/Xcode disponibles.
- UI nueva: pendiente del visto bueno formal de Iluna sobre la paleta aplicada a Ionic.
