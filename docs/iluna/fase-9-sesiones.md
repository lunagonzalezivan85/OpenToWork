# Fase 9 — Sesiones e identidad (rama `iluna-sec-sesiones`)

Continuación del plan de endurecimiento post-auditoría. Resuelve H-04 (refresh token en
localStorage) y añade el interruptor de CAPTCHA server-side; deja el correo de
"empresa verificada" para cerrar el ciclo de confianza de H-40.

## 1. Refresh token → cookie HttpOnly `td_refresh` (H-04)

Antes: `opentowork-refresh-token` vivía en localStorage — al alcance de cualquier XSS
(robo de sesión persistente, el token dura 7 días y rota solo al usarse).

Ahora:

- La API emite `Set-Cookie: td_refresh=<token>; HttpOnly; SameSite=Lax; Path=/api/auth`
  en `login`, `register`, `google/exchange`, `google/signup` y en cada `refresh`
  (rotación). `Secure` se activa solo bajo HTTPS (mismo patrón que `td_google_state`).
- `Path=/api/auth` acota su viaje a los endpoints de autenticación; `SameSite=Lax`
  basta porque portal y API son same-site en dev (`localhost` con puertos distintos)
  y en prod (mismo dominio, `/api` vía proxy).
- `refresh` y `revoke` aceptan el token en el cuerpo (clientes antiguos) **o** en la
  cookie. Un cuerpo `{}` llega con `RefreshToken=""` — el fallback mira `IsNullOrEmpty`,
  no solo null (bug encontrado en las pruebas).
- `revoke` siempre borra la cookie (respuesta `Set-Cookie` expirada).

Cliente (`ApiAuthService` + `AppAuthStateProvider`):

- Las llamadas de auth van con `BrowserRequestCredentials.Include` (necesario para que
  el navegador guarde/envíe la cookie en origen cruzado de desarrollo).
- `PersistAuthAsync` ya no guarda el refresh token en localStorage (y limpia los que
  quedaran de antes).
- El provider hace **renovación silenciosa**: si el access token falta o caducó pero
  hay centinela de sesión (`opentowork-user-id`), intenta `POST /auth/refresh` por
  cookie una vez. El centinela evita que usuarios anónimos disparen el endpoint contra
  el rate limit en cada navegación.
- `RevokeSessionAsync`/`ClearAuthAsync` quedan como la única vía de logout (la ruta
  `/logout`); `MainLayout.HandleLogout` delega en ella.

Compatibilidad: el `access token` sigue en localStorage (ventana de exposición = su
vida útil, 60 min). Moverlo a memoria queda como residual documentado de H-04 —
es un refactor más amplio de `GetTokenAsync`/subidas JS y se valorará aparte.

## 2. CAPTCHA exigible por servidor

- `Recaptcha:Enforced` (appsettings, `false` por defecto): cuando está activo **y** hay
  `SecretKey`, `LoginAsync` exige `LoginDto.RecaptchaToken` válido (401 `captcha`) y
  `send-code` lo exige igual (`400 captcha_required`, `RegistrationCodeRequestDto.
  RecaptchaToken` nuevo, `SendVerificationCodeResult.CaptchaFailed`).
- Sin el flag el comportamiento no cambia — la UI del widget se activará al
  configurar las claves en producción.

## 3. Correo "empresa verificada" (cierre de ciclo H-40)

`CompanyCrmService.SetVerifiedAsync` envía `EmailTemplates.CompanyVerified` al
`ContactEmail` de la empresa solo en la transición falso→verdadero. Best-effort: el
flag queda guardado aunque el SMTP falle. `IEmailService` inyectado en el servicio.

## 4. Dependencia vulnerable (NU1903)

`Microsoft.AspNetCore.DataProtection 8.0.13` arrastraba `System.Security.Cryptography.
Xml 8.0.2` con **8 advisories de gravedad alta**. Pin directo a `10.0.12` en
`OpenToWork.Core` (el proyecto ya targeta net10.0); `dotnet list package --vulnerable`
queda limpio.

## Verificación

| Prueba | Resultado |
|---|---|
| Build solución | 0 errores (warnings preexistentes) |
| `dotnet test` (168) | 163 OK; los 5 fallos son seed-data ausente, iguales a antes del cambio |
| Login → `Set-Cookie` | `td_refresh; httponly; samesite=lax; path=/api/auth; expires +7d` |
| `POST /refresh` solo cookie | 200 y rotación (cookie nueva) |
| `POST /refresh` sin nada | 401 |
| `POST /revoke` | 204 + `Set-Cookie` expirada |
| `POST /refresh` tras revoke | 401 |
| `POST /refresh` con body (legado) | 200 |
| localStorage tras login | sin `opentowork-refresh-token`; `document.cookie` no muestra `td_refresh` (HttpOnly) |
| Borrar access token + recargar `/dashboard` | sesión restaurada por silent refresh |
| `/logout` + `/dashboard` | redirige a `/login`; la sesión no resucita |
| NU1903 | eliminado |

Detalle en `docs/iluna/test_log_2026-10-09.md` (sección S1–S8).
