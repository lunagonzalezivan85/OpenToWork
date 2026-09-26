# Fase 6: Auditoria de Seguridad

**IA:** Iluna
**Rol:** SEC
**Fecha inicio:** 2026-09-27
**Estado:** En progreso (hallazgos reportados; 1 corregido en la misma auditoria)

---

## Resumen ejecutivo

Auditoria completa del aplicativo tras la incorporacion de la configuracion de IA en
`SY_SystemConfig`. Se verifico cobertura de `[Authorize]`, flujo de JWT/refresh, manejo de
secretos, uploads, CORS, dependencias y almacenamiento en cliente. La postura general es
razonable para desarrollo, pero hay hallazgos que **bloquean produccion**.

## Lo que esta bien (verificado)

- Todos los controllers admin heredan `[Authorize(Roles="Admin")]` via `AdminControllerBase`;
  `RequireStaffRole` restringe system-config/email a SuperAdmin.
- Controllers del portal con `[Authorize]`; `AllowAnonymous` solo en endpoints publicos
  intencionales (vacantes publicas, Legal, planes, login/register).
- Mensajeria verifica propiedad de conversacion (`SCUserId == userId`) — sin IDOR para usuarios.
- Upload de CV: solo PDF, limite 10MB, almacenamiento privado fuera de wwwroot, descarga con permiso.
- Passwords con BCrypt; refresh tokens rotan y son revocables (checklist fase 1).
- Swagger solo en `IsDevelopment()`.
- `UpdateBulkAsync` solo edita claves existentes — no permite crear claves arbitrarias.
- El endpoint dedicado `/ai` nunca devuelve la API key (write-only + `HasApiKey`).

## Hallazgos

| ID | Severidad | Archivo | Descripcion | Estado |
|---|---|---|---|---|
| SEC-001 | **Critica** | `src/OpenToWork.API/appsettings.json:13`, `src/OpenToWork.AdminAPI/appsettings.json:16` | Claves de firma JWT hardcodeadas y **commiteadas en un repo publico de GitHub**. Cualquiera con acceso al repo puede forjar tokens validos (bypass total de auth, incluido SuperAdmin). | Pendiente |
| SEC-002 | **Alta** | `src/OpenToWork.Core/Services/SystemConfigService.cs` `GetAllAsync` | El listado general devolvia `smtp_password` y `ai_api_key` en texto plano, anulando el diseno write-only de los endpoints dedicados. | **Corregido** (mask por sufijo `_password`/`_api_key`) |
| SEC-003 | **Alta** | `AuthController`, `AdminAuthController` | Sin rate limiting ni lockout en login/register/refresh — fuerza bruta y credential stuffing ilimitados. | Pendiente |
| SEC-004 | **Alta** | `src/OpenToWork.WEB/Services/ApiAuthService.cs:633-645` | JWT + refresh token en localStorage en texto plano (`opentowork-token`, `opentowork-refresh-token`) — robo trivial via XSS. | Pendiente (documentado desde fase 1) |
| SEC-005 | **Media** | `src/OpenToWork.API/Program.cs`, `src/OpenToWork.AdminAPI/Program.cs` | Falta `UseHttpsRedirection` y headers de seguridad (CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy). | Pendiente |
| SEC-006 | **Media** | `Program.cs` CORS | Origins hardcodeados a localhost con `AllowCredentials` + AnyHeader/AnyMethod. Aceptable en dev; en prod los origins deben venir de config y restringir metodos. | Pendiente |
| SEC-007 | **Media** | `ProfileController.cs:249-251` | Validacion de upload solo por `ContentType` declarado por el cliente (falsificable). Sin verificacion de magic bytes del PDF. | Pendiente |
| SEC-008 | **Media** | `CvParserService` | Con IA activa, los CVs (PII: nombre, contacto, historial laboral) se envian a un proveedor externo. Requiere disclosure en politica de privacidad y DPA con el proveedor (GDPR). | Pendiente |
| SEC-009 | **Media** | `SY_SystemConfig` | `ai_api_key`/`smtp_password` persistidos en texto plano en BD. Recomendado cifrar con `IDataProtection` al escribir y descifrar solo en `Get*CredentialsAsync`. | Pendiente |
| SEC-010 | **Media** | csproj | Dependencias vulnerables: AutoMapper 13.0.1 (Alta, GHSA-rvv3-g6hj-g44x), Microsoft.Extensions.Caching.Memory 8.0.0 (Alta, GHSA-qj66-m88j-hmgj), MailKit/MimeKit 4.9.0 (Moderada). | Pendiente |
| SEC-011 | **Baja** | `CvParserService` / `ProfileController.cs:273-276` | `LogError` incluye el body de respuesta del proveedor (puede contener PII del CV en logs) y el catch del upload traga la excepcion sin registrarla. | Pendiente |

## Prioridad de correccion

### Bloquea produccion (ahora)
1. **SEC-001** — Sacar las JWT keys del repo (user-secrets/variables de entorno) y **rotarlas**:
   las actuales ya estan comprometidas por estar en el historial de Git.
2. **SEC-003** — `AddRateLimiter` por IP+email en login/register/refresh (+ lockout progresivo).
3. SEC-002 — ya corregido en esta sesion.

### Corto plazo
4. **SEC-010** — Actualizar AutoMapper, Caching.Memory, MailKit/MimeKit.
5. **SEC-005** — HTTPS redirect + middleware de headers de seguridad.
6. **SEC-009** — Cifrado de secretos en SY_SystemConfig.

### Media
7. **SEC-004** — Tokens: migrar a cookies HttpOnly o cifrar localStorage (AES-256).
8. **SEC-006** — CORS por configuracion; **SEC-007** magic bytes; **SEC-008** DPA/disclosure;
   **SEC-011** sanitizar logs.

## Fase recomendada

Iniciar **Fase 6 - Seguridad** del workflow del proyecto (etapa de seguridad), con las
correcciones 1-3 como criterio de aceptacion minimo antes de cualquier despliegue.
