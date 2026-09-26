# Fase 6: Auditoria y Correcciones de Seguridad

**IA:** Iluna
**Rol:** SEC
**Fecha:** 2026-09-27
**Estado:** Completada (9/11 hallazgos corregidos, 2 residuales documentados)

---

## Resumen ejecutivo

Auditoria completa del aplicativo tras la incorporacion de la configuracion de IA en
`SY_SystemConfig`. Se verifico cobertura de `[Authorize]`, flujo de JWT/refresh, manejo de
secretos, uploads, CORS, dependencias y almacenamiento en cliente. Correcciones aplicadas
en la misma sesion, ordenadas de menor a mayor complejidad. Build limpio, apps verificadas.

## Lo que esta bien (verificado)

- Todos los controllers admin heredan `[Authorize(Roles="Admin")]` via `AdminControllerBase`;
  `RequireStaffRole` restringe system-config/email a SuperAdmin.
- Controllers del portal con `[Authorize]`; `AllowAnonymous` solo en endpoints publicos
  intencionales (vacantes publicas, Legal, planes, login/register).
- Mensajeria verifica propiedad de conversacion (`SCUserId == userId`) — sin IDOR para usuarios.
- Upload de CV: solo PDF, limite 10MB, almacenamiento privado fuera de wwwroot, descarga con permiso.
- Upload de foto: magic bytes ya validados via `IProfilePhotoStorage.Detect`.
- Passwords con BCrypt; refresh tokens rotan y son revocables (checklist fase 1).
- Swagger solo en `IsDevelopment()`.
- `UpdateBulkAsync` solo edita claves existentes — no permite crear claves arbitrarias.
- El endpoint dedicado `/ai` nunca devuelve la API key (write-only + `HasApiKey`).

## Hallazgos y estado

| ID | Severidad | Archivo | Descripcion | Estado |
|---|---|---|---|---|
| SEC-001 | **Critica** | `appsettings.json` ambas APIs | Claves de firma JWT hardcodeadas y commiteadas en repo publico. | **Corregido**: movidas a user-secrets con claves nuevas aleatorias (rotadas), appsettings vacio, guard de arranque con mensaje claro. RESIDUAL: las viejas quedan en el historial de Git — nunca reutilizarlas; en prod usar `Jwt__Key` env var. |
| SEC-002 | **Alta** | `SystemConfigService.GetAllAsync` | Listado general devolvia `smtp_password`/`ai_api_key` en texto plano. | **Corregido**: mascara por sufijo `_password`/`_api_key`. |
| SEC-003 | **Alta** | `AuthController`, `AdminAuthController` | Sin rate limiting — fuerza bruta ilimitada. | **Corregido**: `AddRateLimiter` con particion por IP. Portal 10 req/min (`auth`), admin 5 req/min (`admin-auth`) en login/register/refresh/forgot/reset/google/recaptcha. Verificado en vivo: 429 al sexto intento. |
| SEC-004 | **Alta** | `LocalStorageService` | JWT + refresh en localStorage en texto plano. | **Corregido**: `SecureValueCipher` (AES-256-GCM, nonce aleatorio + tag) cifra claves que contienen token/secret/password. Valores legacy sin cifrar se leen transparentes. RESIDUAL: un XSS igual puede llamar al JS interop — el cifrado protege el dato en reposo (dump de localStorage/backup del perfil), no la memoria del circuito. Clave en `Security:LocalStorageKey` (efimera si falta). Se elimino `AesEncryptionService` muerto (CBC con IV estatico). |
| SEC-005 | **Media** | `Program.cs` x4 | Sin HTTPS redirect (APIs) ni headers de seguridad. | **Corregido**: `UseHttpsRedirection` en ambas APIs; `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy` en los 4 hosts; CSP en ambos Blazor (permite solo CDNs usados: jsdelivr, unpkg). Script inline del portal movido a `sw-register.js` para no necesitar `'unsafe-inline'` en script-src. |
| SEC-006 | **Media** | `Program.cs` CORS | Origins hardcodeados a localhost. | **Corregido**: `Cors:AllowedOrigins` desde configuracion con fallback a localhost. |
| SEC-007 | **Media** | `ProfileController`, `CandidatesController` | Validacion de upload solo por `ContentType` declarado. | **Corregido**: magic bytes `%PDF-` verificados en ambos endpoints de CV (la foto ya lo tenia). |
| SEC-008 | **Media** | `CvParserService` / legal | Con IA activa, los CVs (PII) viajan a un proveedor externo. | **Pendiente** (documental): agregar clausula de tratamiento por encargado de datos en la politica de privacidad + DPA con el proveedor elegido. |
| SEC-009 | **Media** | `SY_SystemConfig` | `ai_api_key`/`smtp_password` en texto plano en BD. | **Corregido**: cifrados con `IDataProtection` al escribir (prefijo `enc:`), descifrado solo en `Get*CredentialsAsync`. Keyring compartido en `storage/keys` (`SetApplicationName("OpenToWork")`). Valores legacy en claro se leen y se cifran en la proxima escritura. |
| SEC-010 | **Media** | csproj | AutoMapper 13.0.1 (Alta), Caching.Memory 8.0.0 (Alta), MailKit/MimeKit 4.9.0 (Moderada). | **Corregido**: AutoMapper **eliminado** (no se usaba en ningun archivo — ademas v15+ exige licencia comercial), Caching.Memory fijado a 8.0.1, MailKit/MimeKit a 4.18.0. `dotnet list package --vulnerable`: 0 paquetes vulnerables. |
| SEC-011 | **Baja** | `CvParserService`, `ProfileController` | `LogError` con body del proveedor (PII en logs) + catch silencioso en upload. | **Corregido**: solo se loggea status code; el catch ahora hace `LogWarning` con el mensaje. |

## Residuales / proximos pasos

- **SEC-008** (documental): clausula GDPR de subprocesamiento IA en la politica de privacidad.
- Keyring de DataProtection en disco sin cifrar (warning `XmlKeyManager`): en prod agregar
  `ProtectKeysWithDpapi`/cert segun plataforma.
- Rotacion formal de las JWT keys viejas en cualquier entorno donde hayan estado desplegadas.
- reCAPTCHA en login desde dispositivo desconocido (ya existe `verify-recaptcha` — falta UI).

## Build y verificacion

- `dotnet build OpenToWork.slnx` → 0 errores.
- AdminAPI :5001 y AdminWEB :5101 operativos; login admin OK con CSP activo.
- Rate limit verificado: 5 intentos login → 401, 6.º → 429.
