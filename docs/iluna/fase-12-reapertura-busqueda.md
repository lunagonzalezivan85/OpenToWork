# Fase 12 — Reapertura controlada de la búsqueda de candidatos + cierre de H-04

Rama `iluna-sec-rgpd` (apilada sobre `iluna-sec-sesiones`, PR #10).

## H-39: búsqueda reabierta con las tres condiciones

El endpoint `GET /api/candidates/search` y `search/skills` vuelven a funcionar, pero solo
cumpliendo lo que pidió la auditoría:

| Condición | Implementación |
|---|---|
| Empresa verificada | `IsVerifiedCompanyAsync` — solo `PT_Company.IsVerified`. Candidatos, empresas sin verificar y staff → 403 |
| Opt-in del candidato | `CandidateSearchService`/`CompatibilityService` exigen `VisibilityConsentAt` vigente (Fase 10) |
| Datos mínimos | Nombre enmascarado (`María G.`), sin apellido completo ni contacto; score y estado de verificación resumidos |

### Flujo de solicitud (entrega controlada)

La empresa no abre el perfil completo desde la búsqueda: pulsa "Solicitar candidato" →

- `POST /api/candidates/{id}/request` (empresa verificada, candidato con consentimiento,
  anti-duplicados 409, vacante opcional validada como suya) → `PTCandidateRequest` pendiente.
- Cola del staff: AdminAPI `GET/POST /api/admin/candidate-requests` (`handle`/`reject`) +
  página AdminWEB `/candidate-requests` (link directo al pipeline del candidato).
- La entrega real sigue siendo decisión del staff por el pipeline existente
  (`ReadyToDeliver` → `DeliverCandidateAsync` → la empresa ve el perfil completo vía
  `CanViewCandidateAsync`). La solicitud **no desbloquea nada por sí sola**.

Portal: `/candidate-search` reactivada — empresa verificada ve filtros + tarjetas
anonimizadas + botón "Solicitar" (marcado "Solicitud enviada" tras pedirla). Resto de
roles ve el aviso actualizado.

## H-04 residual: access token fuera de localStorage

El token de acceso ya **no se persiste**: vive en memoria del WASM (`_cachedToken`).

- Login/registro/Google/refresh → `PersistAuthAsync` solo mete `user-id`/`role`/`theme`/`lang`
  en localStorage (centinelas no sensibles) y el token en memoria.
- Recarga de página → `GetTokenAsync()`/`SetAuthHeaderAsync()` detectan el centinela y
  renuevan una vez por la cookie `td_refresh` (ya HttpOnly desde Fase 1). Sin centinela no
  se llama al refresh (evita hits de rate limit en visitas anónimas).
- Logout → revoca cookie + limpia memoria + centinelas.
- Las 3 páginas que leían `opentowork-token` directo (`MainLayout`, `Profile`,
  `CompanyProfile`) y el `AppAuthStateProvider` ahora pasan por `GetTokenAsync()`.
- `Cookies.razor` + `COOKIES_Y_PRIVACIDAD.md` actualizados (documentan `td_refresh`).

## Extra: "cerrar sesión en todos los dispositivos"

`POST /api/auth/revoke-all` (autenticado) → `RevokeAllTokensAsync` revoca **todos** los
refresh tokens activos del usuario y borra la cookie actual. Probado: 2 sesiones creadas,
revoke-all devuelve `{revoked:19}` (acumulados de pruebas) y ambos refreshes → 401.
Pendiente: botón en el perfil (visual — necesita visto bueno de Iluna).

## Migraciones

`CandidateRequest`: tabla `PT_CandidateRequests` (company, requester, candidate, vacancy?,
status Pending/Handled/Rejected, notas, revisión).

## Verificación (curl + Playwright)

- Candidato → `search` 403 ✅ · Empresa verificada → 200 con `QA C.` (enmascarado) ✅
- `POST .../request` 204; duplicado 409 ✅ · `GET company/requests` devuelve el id ✅
- Staff: lista pendiente con datos mínimos; `handle` → 204; re-review → 409; anónimo → 401 ✅
- Login Playwright: `opentowork-token`/`refresh-token` ausentes de localStorage; reload
  restaura sesión vía cookie; logout limpia y /dashboard → login ✅
- `dotnet test`: 163/168 (5 fallos preexistentes de seed-data, sin regresión).

## Nota de diseño

La página `/candidate-search` del portal y `/candidate-requests` del admin reutilizan los
componentes del design system (`bento-card`, `ot-btn`, `ot-table`, `admin-status`); pendiente
el visto bueno de Iluna sobre el copy/UX de la solicitud de candidatos.
