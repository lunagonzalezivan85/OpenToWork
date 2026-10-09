# Fase 0: Contención — auditoría externa 08-Oct-2026

**IA:** Iluna · **Rol:** SEC/FS/QA · **Fecha:** 2026-10-09 · **Rama:** `iluna-sec-contencion`
**Estado:** Completada (13/13 pruebas en verde, ver `test_log_2026-10-09.md`)

## Contexto

Auditoría externa de tratodirecto.es (recon pasivo + roles Candidato y Empresa): 49 hallazgos,
1 crítico (H-39: cualquier empresa recién registrada listaba candidatos reales con nombre, ciudad
y métricas de perfilado). Esta fase aplica **solo contención**: cerrar la superficie, sin rediseño.

## Cambios

| Hallazgo | Cambio | Archivos |
|---|---|---|
| H-39 (Crítica) | `candidates/search` y `search/skills` exigen empresa `IsVerified=true` (403) | `API/Controllers/CandidatesController.cs` |
| H-40 | Verificación manual de empresa por el equipo: `PUT /api/admin/company-crm/companies/{id}/verified` + botón/chip en AdminWEB | `Core/Services/CompanyCrmService.cs`, `AdminAPI/Controllers/CompanyCrmController.cs`, `AdminWEB` (Detail.razor, AdminAuthApiService, admin.css, admin.json es/en) |
| H-41 | `send-code` con correo registrado responde igual (aviso al titular, plantilla `RegistrationAttemptNotice`, cooldown anti-spam); el código se consume ANTES del chequeo de duplicado | `Core/Services/AuthService.cs`, `EmailTemplates.cs` |
| H-43 | Empresa también verifica el correo con código al registrarse (un solo flujo); `next`/`returnUrl` rechazan `//` y `/\` | `AuthService.cs`, `WEB/Register.razor`, `VerifyEmail.razor`, `Login.razor` |
| H-31 | Logout real: página `/logout` y menú revocan el refresh token en servidor y limpian storage | `WEB/Pages/Logout.razor`, `Layout/MainLayout.razor`, `ApiAuthService.RevokeTokenAsync` (faltaba header Authorization → siempre 401) |
| H-29/H-30 | Guard de rutas global: `CascadingAuthenticationState` + `AuthorizeRouteView` + `RedirectToLogin`; `[Authorize(Roles=...)]` en 22 páginas privadas; claims `role` en array expandidos; token expirado = anónimo | `WEB/Components/Routes.razor`, `Shared/RedirectToLogin.razor`, `AppAuthStateProvider.cs`, 22 páginas |
| H-05 | `appsettings.json` del WASM sin `localhost`; config dev en `appsettings.Development.json` | `WEB/wwwroot/appsettings*.json` |

## Decisiones

- El gate usa `IsVerified` (bool existente que nadie escribía) + `CompanyStatus` queda igual.
  `IsVerified` se activa solo por staff en el admin (auditado en `CompanyCrm.SetVerified`).
- Eliminado el flujo "empresa verifica después": ahora ambos roles verifican el correo con código
  antes de que exista la cuenta. `EmailVerified=true` desde el alta.
- Las métricas de perfilado siguen visibles para empresas verificadas (producto). El consentimiento
  del candidato (`IsProfilePublic` → opt-in informado, EIPD) queda para Fase 2 (RGPD).
- Vercel no aplica: el stack es ASP.NET + IIS + MySQL; las pruebas se hicieron con Playwright + curl.

## Verificación

- Build `dotnet build OpenToWork.slnx`: **0 errores**.
- Matriz API: anon 401, candidato 403, empresa sin verificar 403, empresa verificada 200,
  revoke 204 + refresh post-revoke 401, send-code existente 204, register sin código 400.
- Playwright: `/dashboard` anónimo → `/login?returnUrl`; candidato → páginas Company → `/`;
  `/logout` → `/login` con storage limpio; `/vacancies` público intacto; mensaje
  `notVerified` traducido en la UI de búsqueda.

## Residuales para próximas fases

- H-04: refresh token en cookie HttpOnly (sigue en localStorage).
- H-10: matriz de autorización completa (aplicaciones, mensajes, entregas) + tests cruzados.
- H-01/H-02: vacante pública con `TD-XXXX` + SEO (ReferenceCode ya implementado en geo-search).
- `System.Security.Cryptography.Xml` 8.0.2: NU1903 alta — actualizar.
