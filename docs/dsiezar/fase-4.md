# Fase 4: Portal Admin (AdminAPI + AdminWEB)

**IA:** Dsiezar
**Rol:** PM (Project Manager)
**Fecha inicio:** 2026-08-12
**Fecha fin:** Pendiente
**Estado:** Etapa 3 cerrada salvo lo bloqueado por Fase 3 - gestion de roles y los 4 items de deuda tecnica completados el 2026-08-29. Solo quedan pendientes verificaciones manuales y revision de validaciones automaticas, bloqueadas por `PTVerification`/`ValidationService` (Fase 3, no iniciada).

---

> **NOTA (2026-08-14, Dsiezar):** Este trabajo se inicio y documento como "Fase 3" (rama `dsiezar-fase-3`, commits `[Fase 3] ...`), siguiendo el roadmap de `README.md` vigente en ese momento (Fase 2 = Portal Candidatos, Fase 3 = Portal Admin).
>
> El 2026-08-14, Iluna reescribio `README.md` y `docs/PLAN_DE_PROYECTO.md` (commit `97cabed`, directo a `main`) agregando un tercer portal (Corporativo) y un motor de scoring/verificaciones, renumerando las fases: **Fase 3 ahora es el "Motor de Evaluacion y Scoring"** (no iniciado) y **el Portal Admin paso a ser Fase 4**. Este documento se renombro de `fase-3.md` a `fase-4.md` y el trabajo continua en la rama `dsiezar-fase-4` para coincidir con el roadmap vigente.
>
> Las menciones a `dsiezar-fase-3` y a mensajes de commit `[Fase 3] ...` mas abajo son historicas (asi se llamaba la rama y asi se numeraban los commits en el momento en que se hizo cada cosa) y no se reescribieron.

---

## Resumen

Fase 4 construye el portal de administracion completo: `OpenToWork.AdminAPI` y `OpenToWork.AdminWEB`. Permite gestionar usuarios, moderar vacantes y solicitudes, ver metricas del sistema, administrar categorias/skills, exportar datos y auditar acciones administrativas.

**Nota de secuencia (historica, escrita cuando esto era "Fase 3"):** Segun `.agents/WORKFLOW.md`, esta fase no puede entrar en Etapa 3 (Implementacion) ni cerrarse hasta que la Fase 2 (Iluna, en `iluna-fase-2`) este en Etapa 7 (Cierre, aprobada por PM+QA+SEC). Esta planificacion avanzo en paralelo sin bloquear a Iluna.

---

## Etapa 1: Planificacion (PM)

**Alcance (segun `README.md` seccion "Fases del Proyecto" y "Como debe continuar el proyecto"):**

- [ ] `OpenToWork.AdminAPI` con JWT independiente del portal principal
- [ ] Gestion de usuarios: activar, desactivar, eliminar (soft delete)
- [ ] Moderacion de vacantes (temporales y permanentes)
- [ ] Dashboard con metricas y estadisticas (usuarios, vacantes, solicitudes)
- [ ] Gestion de categorias y skills (CRUD)
- [ ] Exportacion de datos (CSV/Excel)
- [ ] Log de auditoria admin (quien hizo que accion y cuando)
- [ ] `OpenToWork.AdminWEB` en Blazor con las paginas correspondientes

**Dependencias con Fase 2 (bloqueantes para ciertas tareas):**

| Tarea de Fase 3 | Depende de |
|---|---|
| Moderacion de vacantes permanentes | `PTVacancy` (Iluna, Fase 2) |
| Moderacion/vista de solicitudes | `PTApplication` (Iluna, Fase 2) |
| Metricas de perfiles completos | `PTCandidateExperience/Education/Certification` (Dsiezar, Fase 2 - aun no iniciada) |

**Tareas sin dependencia (se pueden empezar ya):**
- Estructura base de `OpenToWork.AdminAPI` (JWT independiente, Program.cs, appsettings)
- Gestion de usuarios (`SC_Users` ya existe desde Fase 1)
- Gestion de categorias y skills (`PT_Skills` ya existe desde Fase 1)
- Estructura base de `OpenToWork.AdminWEB` (layout, auth, theme/i18n reutilizando `SharedUI`)
- Log de auditoria admin (tabla nueva `AD_AuditLog`)

**Criterios de aceptacion (borrador, sujeto a validacion PM+QA+SEC):**
- [ ] Admin puede loguearse con JWT independiente del portal principal
- [ ] Admin puede activar/desactivar/eliminar usuarios
- [ ] Admin puede moderar (aprobar/rechazar/cerrar) vacantes
- [ ] Dashboard muestra metricas basicas (total usuarios, vacantes activas, solicitudes por estado)
- [ ] Admin puede crear/editar/eliminar categorias y skills
- [ ] Admin puede exportar listado de usuarios/vacantes a CSV
- [ ] Toda accion administrativa queda registrada en `AD_AuditLog`
- [ ] Build sin errores, i18n completo (es/en), QA y SEC aprueban

**Riesgos identificados:**
- Moderacion de vacantes/solicitudes no se puede implementar ni probar end-to-end hasta que Iluna cierre Fase 2.
- Se puede mitigar construyendo esas pantallas contra datos mock o adelantando solo la base del AdminAPI/AdminWEB mientras tanto.

- [x] Aprobacion para pasar a Etapa 2 (Diseno Tecnico)

---

## Etapa 2: Diseno Tecnico (PM + FS)

**Contexto:** Al momento de escribir esto, `main` ya trae mergeado gran parte de Fase 2 (Iluna): `PTVacancy`, `PTApplication`, `PTCandidateExperience/Education/Certification`, `ProfileController`, `PermanentVacanciesController`, `ApplicationsController`, 2 migraciones (`Phase2`, `Phase2Security`). Esto **desbloquea** varias tareas de Fase 3 que antes dependian de Fase 2 (moderacion de vacantes permanentes y solicitudes, metricas de perfiles).

`OpenToWork.AdminAPI` y `OpenToWork.AdminWEB` siguen siendo scaffolding puro (sin JWT, sin DbContext, sin controllers reales; `AdminWEB` todavia con paginas de plantilla `Counter.razor`/`Weather.razor` y Bootstrap). Todo el diseño de abajo parte de cero en ambos proyectos.

### Entidades nuevas/modificadas

- `AD_AuditLog` (nueva) - segun `docs/DATABASE_DESIGN.md` seccion 5.1:
  - `Id`, `SC_UserId` (FK admin que actua), `Action` (VARCHAR 100, ej. "DeactivateUser"), `EntityType` (VARCHAR 100), `EntityId` (nullable), `ChangesJson` (LONGTEXT, before/after), `IpAddress` (VARCHAR 45), + auditoria estandar (`BaseEntity`)
  - Indices: `(SC_UserId, IsDeleted)`, `(EntityType, EntityId, IsDeleted)`, `(CreatedAt, IsDeleted)`
- No se tocan entidades existentes de `PT_`/`SC_`; el AdminAPI las consulta a traves de `AppDbContext` compartido con el portal principal, solo agrega `AD_AuditLog`.

### Endpoints nuevos (`OpenToWork.AdminAPI`)

| Controller | Endpoints | Descripcion |
|---|---|---|
| `AdminAuthController` | `POST /api/admin/auth/login` | Login exclusivo admin (`SC_Users` con `PrimaryRole = Admin`), JWT propio (key/issuer/audience distintos al portal principal) |
| `UsersController` | `GET /api/admin/users`, `GET /api/admin/users/{id}`, `PUT /api/admin/users/{id}/activate`, `PUT /api/admin/users/{id}/deactivate`, `DELETE /api/admin/users/{id}` (soft delete) | Gestion de usuarios |
| `VacanciesController` (admin) | `GET /api/admin/vacancies`, `PUT /api/admin/vacancies/{id}/moderate` (aprobar/rechazar/cerrar) | Moderacion de `PT_Vacancies` y `PT_TempVacancies` |
| `ApplicationsController` (admin) | `GET /api/admin/applications` | Vista de solicitudes (solo lectura, sin cambiar estado - eso es del candidato/empresa) |
| `SkillsController` (admin) | `GET/POST/PUT/DELETE /api/admin/skills` | CRUD de `PT_Skills` (categorias/skills) |
| `DashboardController` | `GET /api/admin/dashboard/metrics` | Conteos: usuarios activos, candidatos, empresas, vacantes por estado, solicitudes por estado |
| `AuditLogController` | `GET /api/admin/audit-log` | Consulta de `AD_AuditLog` (solo lectura, paginado, filtros por usuario/entidad/fecha) |
| `ExportController` | `GET /api/admin/export/users`, `GET /api/admin/export/vacancies` | Exportacion CSV |

Cada accion de escritura (activar/desactivar/eliminar/moderar) debe registrar una fila en `AD_AuditLog` (Action, EntityType, EntityId, ChangesJson, IpAddress).

### Componentes/paginas nuevas (`OpenToWork.AdminWEB`)

- `Components/Pages/AdminLogin.razor` - login independiente
- `Components/Layout/AdminLayout.razor` - reemplaza el layout de plantilla, reutiliza estilos/temas de `SharedUI` (no Bootstrap)
- `Components/Pages/Dashboard.razor` - metricas con `BentoCard`
- `Components/Pages/Users.razor` - tabla de usuarios con activar/desactivar/eliminar
- `Components/Pages/VacanciesModeration.razor` - moderar vacantes
- `Components/Pages/ApplicationsView.razor` - ver solicitudes
- `Components/Pages/Skills.razor` - CRUD de skills/categorias
- `Components/Pages/AuditLog.razor` - tabla de auditoria con filtros
- Eliminar paginas de plantilla: `Counter.razor`, `Weather.razor`

### Configuracion base pendiente (antes de las paginas)

- `AdminAPI/Program.cs`: JWT Bearer (config propia), `AppDbContext` (misma BD MySQL), CORS, DI de servicios admin, Swagger ya existe
- `AdminAPI/appsettings.json`: agregar `ConnectionStrings`, `Jwt` (key/issuer/audience **distintos** a `OpenToWork.API`, ej. issuer `OpenToWork.Admin`)
- `AdminWEB/Program.cs`: DI de `HttpClient` hacia `AdminAPI` (puerto 5001), `AuthStateProvider` propio, quitar referencias a Bootstrap, agregar referencia a `SharedUI` y temas
- `AdminWEB/wwwroot`: quitar carpeta `lib/bootstrap`, cargar CSS de `SharedUI`/temas del portal principal

### Migraciones

- `AdminAuditLog` - agrega solo `AD_AuditLog`. Se genera **despues** de que Iluna cierre su migracion `Phase2Security` (ya esta en `main`), asi que no hay conflicto de migracion concurrente.

### Riesgo re-evaluado

Con el merge de `main`, la unica dependencia real que queda es de **datos**, no de disenio: `ApplicationsController` (admin, solo lectura) puede implementarse ya contra `PT_Applications`, que ya existe. Sin bloqueos pendientes para iniciar Etapa 3.

- [x] Aprobacion PM + FS para pasar a Etapa 3 (Implementacion)

---

## Etapa 3: Implementacion (FS)

**Alcance implementado en esta iteracion:** base de `OpenToWork.AdminAPI` (JWT independiente + login admin + auditoria). Quedan pendientes para la siguiente iteracion: `UsersController`, `VacanciesController`, `SkillsController`, `DashboardController`, `ExportController` y todo `OpenToWork.AdminWEB`.

**Archivos creados:**

| Archivo | Descripcion |
|---|---|
| `src/OpenToWork.Models/Entities/ADAuditLog.cs` | Entidad `AD_AuditLog` |
| `src/OpenToWork.Core/Interfaces/IAdminAuthService.cs` | Contrato login admin |
| `src/OpenToWork.Core/Interfaces/IAuditLogService.cs` | Contrato registro/consulta de auditoria |
| `src/OpenToWork.Core/Services/AdminAuthService.cs` | Login restringido a `PrimaryRole = Admin`, JWT firmado con config propia de `AdminAPI` |
| `src/OpenToWork.Core/Services/AuditLogService.cs` | `LogAsync` (escribe fila) + `GetLogsAsync` (paginado) |
| `src/OpenToWork.Shared/DTOs/AuditLogDto.cs` | DTO de auditoria |
| `src/OpenToWork.AdminAPI/Controllers/AdminAuthController.cs` | `POST /api/admin/auth/login` |
| `src/OpenToWork.AdminAPI/Controllers/AuditLogController.cs` | `GET /api/admin/audit-log` (solo rol Admin) |
| `src/OpenToWork.Models/Migrations/20260812214205_AdminAuditLog.cs` | Migracion: crea tabla `AD_AuditLogs` |

**Archivos modificados:**

| Archivo | Cambio |
|---|---|
| `src/OpenToWork.Models/Context/AppDbContext.cs` | `DbSet<ADAuditLog>` + configuracion de tabla/indices |
| `src/OpenToWork.Core/Extensions/ServiceCollectionExtensions.cs` | Nuevo `AddAdminCoreServices()` (no reutiliza `AddCoreServices()` para no exponer servicios del portal principal en el AdminAPI) |
| `src/OpenToWork.AdminAPI/Program.cs` | JWT Bearer con config propia, `AppDbContext`, CORS hacia `AdminWEB`, Swagger con Bearer |
| `src/OpenToWork.AdminAPI/appsettings.json` | `ConnectionStrings` + `Jwt` (issuer/audience/key **distintos** a `OpenToWork.API`, expiracion mas corta: 60 min / refresh 1 dia) |
| `src/OpenToWork.AdminAPI/Properties/launchSettings.json` | Puerto alineado al README: 5001 (antes 5087, scaffolding por defecto) |
| `src/OpenToWork.AdminWEB/Properties/launchSettings.json` | Puerto alineado al README: 5101 (antes 5018) |

**Claves i18n agregadas:** ninguna (sin UI todavia).

**Build:** `dotnet build OpenToWork.slnx` -> 0 errores, 20 advertencias (preexistentes: AutoMapper/Caching.Memory NU1903, paquetes Blazor NU1510/NU1603 - ninguna introducida por estos cambios).

**Migracion:** `AdminAuditLog` creada y **aplicada**. MySQL corre via XAMPP (`C:\xampp\mysql`, root sin password, coincide con `appsettings.json`). Se creo la base `OpenToWorkDb` y se aplicaron las 4 migraciones pendientes con `dotnet-ef database update`: `InitialCreate`, `Phase2`, `Phase2Security` (Iluna) y `AdminAuditLog` (Dsiezar). Verificado: 19 tablas en `OpenToWorkDb`, incluida `ad_auditlogs`.

**Nota de entorno:** este equipo no tenia el SDK de .NET instalado (solo runtimes) ni fuente de NuGet configurada. Se instalo `Microsoft.DotNet.SDK.10` via `winget` y se agrego el source `nuget.org` para poder compilar y generar la migracion. MySQL se resolvio usando el XAMPP ya instalado en `C:\xampp` (mysqld corriendo en :3306).

---

## Etapa 3 (continuacion): OpenToWork.AdminWEB

**Alcance implementado:** base completa de `AdminWEB` (antes scaffolding con Bootstrap/plantilla sin tocar) - login, layout, dashboard y pagina de auditoria, consumiendo el `AdminAPI` real.

**Archivos creados:**

| Archivo | Descripcion |
|---|---|
| `Services/LocalStorageService.cs` | Wrapper de `localStorage` via JSInterop (mismo patron que `OpenToWork.WEB`) |
| `Services/AdminAuthApiService.cs` | Cliente HTTP hacia `AdminAPI`: login, audit-log, manejo de token (`otwadmin-token`, namespacing propio para no chocar con el portal principal) |
| `Services/AdminAuthStateProvider.cs` | Parseo de claims del JWT (no se usa con `AuthorizeView`, ver nota de bug abajo) |
| `Services/LanguageService.cs` | i18n simplificado a una sola seccion (`admin.json`) |
| `wwwroot/config/language/{es,en}/admin.json` | Claves de traduccion del admin |
| `Components/Layout/AdminLayout.razor` | Layout autenticado: nav (Panel, Auditoria), selector de tema/idioma, logout |
| `Components/Layout/LoginLayout.razor` | Layout minimo para `/login` |
| `Components/Pages/Login.razor` | Login contra `POST /api/admin/auth/login` |
| `Components/Pages/Dashboard.razor` | Pagina `/`, BentoCards, guard manual de sesion |
| `Components/Pages/AuditLog.razor` | Pagina `/audit-log`, consume `GET /api/admin/audit-log` |

**Archivos modificados/eliminados:**

| Archivo | Cambio |
|---|---|
| `wwwroot/css/`, `wwwroot/themes/`, `wwwroot/js/theme-switcher.js`, `wwwroot/js/language-switcher.js` | Copiados de `OpenToWork.WEB` para reusar el sistema de diseño (en vez de Bootstrap) |
| `wwwroot/lib/bootstrap/*`, `wwwroot/app.css` | Eliminados (scaffolding no usado) |
| `Components/Pages/Counter.razor`, `Weather.razor`, `Home.razor` | Eliminados (paginas de plantilla) |
| `Components/Layout/MainLayout.razor(.css)`, `NavMenu.razor(.css)` | Eliminados, reemplazados por `AdminLayout`/`LoginLayout` |
| `Components/App.razor` | Quita Bootstrap, referencia el CSS compartido y los temas; `Routes` con `prerender: false` (ver bug) |
| `Components/Routes.razor` | Router simple (`RouteView`), sin `AuthorizeRouteView` (ver bug) |
| `Components/Pages/NotFound.razor` | Layout actualizado a `AdminLayout` |
| `Components/_Imports.razor` | Agrega usings de autorizacion (no usados activamente, ver bug) |
| `Program.cs` | DI de servicios admin, `HttpClient` hacia `AdminAPI` (puerto 5001) |
| `appsettings.json` | `ApiSettings.BaseUrl` |
| `wwwroot/css/components.css` | Fix: `#blazor-error-ui` sin `display: none` (bug heredado de `OpenToWork.WEB`, ver abajo) |

**Bugs encontrados y corregidos durante la verificacion en navegador:**

1. **`[Authorize]` + `AuthorizeRouteView` con Blazor Server rompe todo el sitio.** Al usar `@attribute [Authorize]` en paginas (Dashboard, AuditLog) junto con `CascadingAuthenticationState`/`AuthorizeRouteView`, ASP.NET Core inserta automaticamente middleware de autenticacion (porque `AddAuthentication()`/`AddAuthorization()` estaban registrados) y falla con `InvalidOperationException: No authenticationScheme was specified` en **cualquier** request, porque no hay default challenge scheme configurado. Se reemplazo por un guard manual en `OnInitializedAsync` de cada pagina (leer token de `localStorage`, redirigir a `/login` si falta) - mismo patron ya usado (de forma mas laxa) en `OpenToWork.WEB`.
2. **Prerendering + JSInterop causaba redirect-loop a `/login` aun autenticado.** El guard manual corre tambien durante el prerender (antes de que el circuito interactivo conecte), momento en el que `localStorage` no esta disponible via JSInterop (`InvalidOperationException` silenciosa) y el guard interpretaba "sin token" incorrectamente. Se soluciono deshabilitando el prerender del router: `<Routes @rendermode="new InteractiveServerRenderMode(prerender: false)" />`.
3. **Banner rojo `#blazor-error-ui` siempre visible.** `components.css` (copiado de `OpenToWork.WEB`) define el estilo visual de `#blazor-error-ui` pero **nunca** con `display: none`, asi que el banner de error de Blazor aparece siempre, no solo cuando hay un error real. Se corrigio en la copia de `AdminWEB`. **Este mismo bug probablemente existe en `OpenToWork.WEB`** (Iluna) ya que comparte el mismo `components.css` y el mismo patron de layout - no se toco ese archivo por estar fuera del alcance de esta rama, pero vale la pena reportarlo.

**Verificacion end-to-end (navegador + MySQL real, no mocks):**
- Se instalo el SDK de .NET 10 (`winget`) y se configuro `nuget.org` como fuente (no habia ninguna).
- Se detecto MySQL corriendo via XAMPP (`C:\xampp\mysql`, root sin password) y se aplicaron las 4 migraciones pendientes (`InitialCreate`, `Phase2`, `Phase2Security`, `AdminAuditLog`) con `dotnet-ef database update`.
- Se creo un usuario admin de prueba (`admin@opentowork.com`) directamente en `SC_Users` (bcrypt hash generado con un proyecto de consola descartable, ya que `POST /api/auth/register` del portal principal esta roto - ver bug #4).
- Probado con `curl` contra `AdminAPI` real: login correcto -> 200 + JWT (issuer `OpenToWork.Admin`); password incorrecta -> 401; cuenta no-admin con password correcta -> 401 ("Account is not an administrator"); `GET /api/admin/audit-log` sin token -> 401, con token -> 200 `[]`.
- Probado en navegador contra `AdminWEB` real: login -> redirige a `/` con Dashboard renderizado: navegacion a `/audit-log` funciona; logout limpia el token y redirige a `/login`.
- `dotnet build OpenToWork.slnx` -> 0 errores.

**Bug #4 (fuera de alcance, no corregido):** `OpenToWork.API/Program.cs` lee `Google:ClientId` pero `appsettings.json` define la seccion como `GoogleOAuth`, entonces Google Auth se registra con `ClientId` vacio y **cualquier** request a `OpenToWork.API` (incluida `/api/auth/register`) lanza `ArgumentException`. Es codigo de Iluna (Fase 1), fuera del alcance de esta rama - se documenta aqui para que se corrija por separado.

---

## Etapa 3 (continuacion): Controllers de gestion

**Alcance implementado:** `UsersController`, `VacanciesController` (moderacion), `SkillsController` (CRUD), `DashboardController` (metricas reales) en `AdminAPI`, y sus paginas correspondientes en `AdminWEB` (`/users`, `/vacancies`, `/skills`, dashboard con metricas reales en `/`).

**Archivos creados:**

| Archivo | Descripcion |
|---|---|
| `Shared/DTOs/AdminDtos.cs` | `AdminUserDto`, `AdminVacancyDto`, `ModerateVacancyDto`, `AdminSkillDto`, `CreateSkillDto`, `DashboardMetricsDto` |
| `Core/Interfaces/IAdminUserService.cs`, `IAdminVacancyService.cs`, `IAdminSkillService.cs`, `IAdminDashboardService.cs` | Contratos |
| `Core/Services/AdminUserService.cs` | Listar/activar/desactivar/eliminar (soft delete) usuarios, cada accion de escritura registra `AD_AuditLog` |
| `Core/Services/AdminVacancyService.cs` | Lista combinada `PT_Vacancies` + `PT_TempVacancies`; moderar cambia `Status` (permanentes) o `IsPublished` (temporales) |
| `Core/Services/AdminSkillService.cs` | CRUD de `PT_Skills` |
| `Core/Services/AdminDashboardService.cs` | Conteos reales: usuarios, candidatos, empresas, vacantes por estado, solicitudes por estado, skills, entradas de auditoria |
| `AdminAPI/Controllers/AdminControllerBase.cs` | Base con `[Authorize(Roles="Admin")]`, helpers `AdminId`/`ClientIp` para auditoria (refactor: `AuditLogController` tambien la usa ahora) |
| `AdminAPI/Controllers/UsersController.cs`, `VacanciesController.cs`, `SkillsController.cs`, `DashboardController.cs` | Endpoints admin |
| `AdminWEB/Components/Pages/Users.razor` | Tabla de usuarios con activar/desactivar/eliminar |
| `AdminWEB/Components/Pages/Vacancies.razor` | Tabla de vacantes (permanentes + temporales) con aprobar/cerrar |
| `AdminWEB/Components/Pages/Skills.razor` | Alta/baja de skills |

**Archivos modificados:**

| Archivo | Cambio |
|---|---|
| `Core/Extensions/ServiceCollectionExtensions.cs` | Registra los 4 servicios nuevos en `AddAdminCoreServices` |
| `AdminWEB/Services/AdminAuthApiService.cs` | Metodos HTTP para users/vacancies/skills/dashboard |
| `AdminWEB/Components/Layout/AdminLayout.razor` | Nav: Usuarios, Vacantes, Skills |
| `AdminWEB/Components/Pages/Dashboard.razor` | Metricas reales (antes texto estatico) |
| `wwwroot/config/language/{es,en}/admin.json` | Claves nuevas: `users.*`, `vacancies.*`, `skills.*`, `dashboard.*` ampliado |

**Build:** `dotnet build OpenToWork.slnx` -> 0 errores (se encontraron y corrigieron 3 errores `CS0542` por nombrar campos privados igual que la clase del componente: `Users`/`Vacancies`/`Skills` -> `UsersList`/`VacanciesList`/`SkillsList`).

**Verificacion end-to-end en navegador (datos reales, no mocks):** se sembraron un usuario candidato, un usuario+perfil de empresa y una vacante en borrador directamente via SQL para probar sin afectar la sesion admin propia. Se probo en `AdminWEB`:
- Dashboard: metricas coincidieron exactamente con los datos sembrados (3 usuarios, 3 activos, 0 candidatos [sin perfil `PT_Candidate`], 1 empresa, 1 vacante permanente, 0 skills antes de la prueba).
- `/users`: desactivar `testcandidate@opentowork.com` -> boton cambia a "Activar", estado a "Inactivo".
- `/vacancies`: aprobar la vacante en "Borrador" -> pasa a "Activa", boton "Aprobar" desaparece.
- `/skills`: crear skill "C#" / categoria "Backend" -> aparece en la tabla.
- `/audit-log`: las 3 acciones anteriores quedaron registradas con el email del admin, accion, entidad y fecha correctos.

**Nota:** los datos de prueba (`testcandidate@opentowork.com`, `testcompany@opentowork.com`, la vacante y el skill "C#") quedaron en la base de datos local para que sirvan de fixture al seguir probando manualmente.

---

## Fix fuera de alcance de Fase 3 (a peticion del usuario, 2026-08-13)

Los 2 bugs preexistentes documentados arriba (encontrados durante la verificacion de Fase 3, pero viviendo en codigo de Fase 1/2 de Iluna) se corrigieron a pedido explicito del usuario, en vez de solo reportarlos:

1. **`src/OpenToWork.API/Program.cs`:** el authentication builder leia `Google:ClientId`/`Google:ClientSecret` pero `appsettings.json` define la seccion como `GoogleOAuth`. Se corrigio la clave de configuracion (`GoogleOAuth:ClientId`/`GoogleOAuth:ClientSecret`) y ademas se hizo que `.AddGoogle(...)` solo se registre si hay credenciales configuradas, para que el API no vuelva a romperse la proxima vez que `GoogleOAuth` este vacio (estado actual, ya que aun no se integra el flujo real). Verificado con `curl POST /api/auth/register` -> 200 (antes tiraba 500 en cualquier request).
2. **`src/OpenToWork.WEB/wwwroot/css/components.css`:** mismo fix que en `AdminWEB` - se agrego `display: none` (+ `position: fixed` y estilos de `.dismiss`) a `#blazor-error-ui`, que antes quedaba siempre visible. Verificado en navegador: `getComputedStyle` confirma `display: none` por defecto.

**Riesgo asumido:** ambos archivos siguen siendo activamente modificados por Iluna en su rama. Es posible que al mergear `dsiezar-fase-3` y `iluna-fase-2` a `main` haya un conflicto de merge en estas 2 lineas — es un conflicto trivial de resolver (no logica de negocio en disputa), pero se avisa aqui para que no sea sorpresa.

---

## Etapa 3 (cierre del alcance original): ExportController

**Alcance implementado:** exportacion CSV de usuarios y vacantes, ultima pieza pendiente del diseno de Etapa 2.

**Archivos creados:**

| Archivo | Descripcion |
|---|---|
| `Core/Interfaces/IExportService.cs` | Contrato: `ExportUsersCsvAsync`, `ExportVacanciesCsvAsync` |
| `Core/Services/ExportService.cs` | Genera CSV reutilizando `IAdminUserService`/`IAdminVacancyService` (sin duplicar queries), con escape basico de comas/comillas |
| `AdminAPI/Controllers/ExportController.cs` | `GET /api/admin/export/users`, `GET /api/admin/export/vacancies` - devuelven `text/csv` como archivo descargable; cada exportacion se registra en `AD_AuditLog` |
| `AdminWEB/wwwroot/js/file-download.js` | Helper JS: crea un link con `data:` URI en base64 y dispara la descarga en el navegador (Blazor Server no puede hacer un `<a href>` directo autenticado, asi que el CSV se trae por HTTP server-side con el JWT y se entrega al navegador via JSInterop) |

**Archivos modificados:**

| Archivo | Cambio |
|---|---|
| `Core/Extensions/ServiceCollectionExtensions.cs` | Registra `IExportService` |
| `AdminWEB/Services/AdminAuthApiService.cs` | `ExportUsersCsvAsync`/`ExportVacanciesCsvAsync` (devuelven `byte[]`) |
| `AdminWEB/Components/App.razor` | Referencia a `file-download.js` |
| `AdminWEB/Components/Pages/Users.razor`, `Vacancies.razor` | Boton "Exportar CSV" |
| `wwwroot/config/language/{es,en}/admin.json` | Clave `exportCsv` en `users`/`vacancies` |

**Build:** `dotnet build OpenToWork.slnx` -> 0 errores.

**Verificacion end-to-end:** `curl` contra `AdminAPI` confirmo el CSV correcto para usuarios y vacantes. En el navegador, se hizo login, se fue a `/users`, se hizo clic en "Exportar CSV" sin errores de consola, y se confirmo en `/audit-log` una entrada nueva `ExportUsers` generada por ese clic (ademas de las que ya habian quedado de las pruebas por `curl`).

---

---

## Etapa 4 + 5 (QA + SEC): Revision de codigo

Revision de todo el diff de `dsiezar-fase-3` vs `main` (correctitud + seguridad + limpieza), 8 angulos de busqueda independientes + verificacion 1 a 1 de cada hallazgo contra el codigo real corriendo (no solo lectura de codigo). 10 hallazgos confirmados, 6 de correctitud/seguridad corregidos de inmediato, 4 de limpieza/arquitectura documentados como deuda tecnica.

### Corregidos

| # | Archivo | Bug | Fix |
|---|---|---|---|
| 1 | `AdminAuthService.cs:36` | El check de rol Admin corria **antes** de verificar la contrasena -> cualquiera podia confirmar si un email es una cuenta activa no-admin sin contrasena valida (enumeracion de cuentas, sin autenticacion) | Se movio el check de rol **despues** de `BCrypt.Verify`, y el mensaje de rol incorrecto ahora es el mismo "Invalid credentials" que el de password incorrecta |
| 2 | `AdminUserService.cs`, `AdminVacancyService.cs`, `AuditLogService.cs` | `page<=0` generaba un `Skip()` negativo -> MySQL rechazaba el OFFSET negativo -> 500 con stack trace completo filtrado al cliente | `page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 1_000_000);` al inicio de cada metodo |
| 3 | `ExportService.cs` (CsvField) | CSV export no escapaba caracteres `=+-@` al inicio de un campo -> formula injection si el CSV se abre en Excel (candidato/empresa puede envenenar su propio Title/Location, sin necesitar acceso admin) | Prefijo `'` en campos que empiezan con esos caracteres (mitigacion estandar), mas quoting tambien en `\r` (antes solo `\n`) |
| 4 | `AdminVacancyService.cs` (`ModerateAsync`) | Cerrar una vacante temporal solo ponia `IsPublished=false`, quedando identica a "nunca revisada" (Draft) -> un admin podia re-aprobar en silencio algo que ya habia cerrado | `PT_TempVacancy` no tiene campo para "Cerrada"; se trata como terminal: se hace soft-delete (`IsDeleted/DeletedAt/DeletedBy`) ademas de despublicar, asi desaparece del listado en vez de quedar ambigua |
| 5 | `UsersController.cs` | Ningun admin podia ser bloqueado de desactivarse/eliminarse a si mismo -> riesgo de bloqueo total si es el unico admin | `if (id == AdminId) return Conflict(...)` en `/deactivate` y `DELETE` |
| 6 | `AuditLog.razor:15` | Estado de carga usaba `admin.login.loading` ("Iniciando sesion...") en vez de `admin.common.loading` ("Cargando...") | Clave corregida |

Los 6 fixes se verificaron corriendo contra el `AdminAPI` real (no solo lectura de codigo): `curl` confirmo que login con password incorrecta ahora siempre devuelve el mismo mensaje generico; `page=0`/`page=-5` devuelven 200 con datos en vez de 500; export de una vacante con titulo `=HYPERLINK(...)` sale como `'=HYPERLINK(...)` entre comillas; cerrar una vacante temporal la hace desaparecer del listado en vez de volver a "Borrador"; el propio admin recibe 409 al intentar desactivarse/eliminarse.

### Documentado como deuda tecnica (no corregido en esta sesion)

| # | Archivo | Hallazgo |
|---|---|---|
| 7 | `AdminAuthService.cs` | Genera JWT/refresh token con logica casi identica a `AuthService.cs` (portal principal), copiada en vez de compartida - riesgo de que ambas implementaciones diverjan si se endurece el hashing en una y no en la otra |
| 8 | `AdminVacancyService.cs` | `GetVacanciesAsync` (y el export que lo reusa) carga las tablas `PT_Vacancies`/`PT_TempVacancies` **completas** en memoria antes de paginar - funciona bien con pocos datos, no escala |
| 9 | `AdminWEB/Services/LocalStorageService.cs`, `LanguageService.cs` | Copias casi identicas de los servicios homonimos de `OpenToWork.WEB`, sin un proyecto compartido (tipo `SharedUI`) que las contenga una sola vez |
| 10 | `AdminWEB/Components/Pages/*.razor` | El guard "leer token de localStorage, redirigir a /login si falta" esta copiado y pegado en las 5 paginas en vez de vivir una sola vez en `AdminLayout` - fragil si se agrega una pagina nueva y se olvida el guard |

> **Resueltos los 4 (2026-08-29, Dsiezar):** ver seccion "Etapa 3 (cierre): Gestion de roles + resolucion de deuda tecnica" mas abajo para el detalle de cada fix y su verificacion.

---

## Resumen de Cambios

Fase 3 completa segun el diseno original de Etapa 2: Etapa 1 (Planificacion), Etapa 2 (Diseno Tecnico), Etapa 3 (Implementacion completa - AdminAPI con 7 controllers: Auth, Users, Vacancies, Skills, Dashboard, AuditLog, Export; AdminWEB con las 6 paginas correspondientes), y una revision de codigo tipo QA+SEC que encontro y corrigio 6 bugs reales (incluyendo un problema de enumeracion de cuentas, un 500 con paginacion negativa, CSV injection, y perdida de estado en moderacion de vacantes temporales), dejando 4 items de deuda tecnica documentados. Portal Admin funcional de punta a punta contra MySQL real, verificado en navegador/curl en cada entrega y en cada fix, no solo compilado. Adicionalmente, a peticion del usuario, se corrigieron 2 bugs preexistentes fuera de alcance en `OpenToWork.API` (Google OAuth) y `OpenToWork.WEB` (banner de error).

**Pendiente antes de poder cerrar formalmente la fase (Etapa 7):** aprobacion formal de PM (mas alla de la revision de codigo ya hecha), y decidir si los 4 items de deuda tecnica de la Etapa 4+5 se abordan antes del merge o se dejan para una fase posterior.

---

## Etapa 3 (extension): ApplicationsController - vista de solicitudes

Feature disenada en Etapa 2 (`docs/dsiezar/fase-3.md` seccion de endpoints) pero no implementada en la primera pasada de Etapa 3. Se completa ahora: vista de solo lectura de `PT_Applications` para el admin (sin cambiar estado - eso sigue siendo del candidato/empresa).

**Archivos creados:**

| Archivo | Descripcion |
|---|---|
| `Shared/DTOs/AdminApplicationDto.cs` | Nombre/email del candidato, titulo de la vacante, estado, fecha |
| `Core/Interfaces/IAdminApplicationService.cs`, `Core/Services/AdminApplicationService.cs` | Query con joins a `PT_Candidates`/`SC_Users`/`PT_Vacancies`, paginacion clamped desde el inicio (aplicando la leccion de la revision anterior) |
| `AdminAPI/Controllers/ApplicationsController.cs` | `GET /api/admin/applications` (solo lectura) |
| `AdminWEB/Components/Pages/Applications.razor` | Tabla de solicitudes en `/applications` |

**Archivos modificados:** `ServiceCollectionExtensions.cs` (registro), `AdminAuthApiService.cs` (cliente HTTP), `AdminLayout.razor` (nav link), `admin.json` es/en (seccion `applications`).

**Verificacion:** se sembro un candidato (`PT_Candidates`) y una solicitud (`PT_Applications`) via SQL. `curl` confirmo el join correcto (nombre completo, email, titulo de vacante). En navegador: login -> `/applications` muestra "Juan Perez (testcandidate@opentowork.com) / Desarrollador Backend Senior / Pendiente / 14/08/2026", y el link "Solicitudes" aparece en el nav junto a los demas.

**Build:** `dotnet build OpenToWork.slnx` -> 0 errores.

Con esto, `AdminAPI` queda con **8 controllers** (Auth, Users, Vacancies, Applications, Skills, Dashboard, AuditLog, Export) y `AdminWEB` con **7 paginas**.

---

## Etapa 3 (cierre): Gestion de roles + resolucion de deuda tecnica (2026-08-29)

**Contexto:** a peticion del usuario ("terminemos lo que falta de la fase 4"), se retoma el checklist pendiente de la Etapa 1. De los 3 items no marcados, 2 siguen bloqueados por Fase 3 (`PTVerification`/`ValidationService` no existen todavia); se implementa el tercero (gestion de roles) y se resuelven los 4 items de deuda tecnica de la revision QA+SEC (items 7-10 de la tabla de arriba).

### Gestion de roles de usuario

**Archivos creados/modificados:**

| Archivo | Cambio |
|---|---|
| `Shared/DTOs/AdminDtos.cs` | Nuevo `ChangeRoleDto { int Role }` |
| `Core/Interfaces/IAdminUserService.cs` | Nuevo `Task<bool> ChangeRoleAsync(Guid id, int newRole, Guid adminId, string? ipAddress)` |
| `Core/Services/AdminUserService.cs` | Implementacion: valida el rol con `Enum.IsDefined`, actualiza `SCUser.PrimaryRole`, agrega una fila a `SC_UserRoles` si el rol nuevo no esta en el historial (nunca borra roles previos), registra auditoria `ChangeUserRole` con `{"from":X,"to":Y}` |
| `AdminAPI/Controllers/UsersController.cs` | `PUT /api/admin/users/{id}/role`, con el mismo patron de auto-bloqueo que `/deactivate` y `DELETE` (`if (id == AdminId) return Conflict(...)`) mas `BadRequest` si el rol no es valido |
| `AdminWEB/Services/AdminAuthApiService.cs` | `ChangeUserRoleAsync(Guid id, int role)` |
| `AdminWEB/Components/Pages/Users.razor` | Selector `<select>` de rol por tarjeta (deshabilitado para la propia cuenta del admin, usando el `otwadmin-user-id` ya guardado en `localStorage`), con `confirm()` antes de aplicar el cambio y recarga de la lista despues |
| `wwwroot/config/language/{es,en}/admin.json` | Claves nuevas: `changeRole`, `confirmRoleChange`, `roleCandidate`, `roleCompany`, `roleAdmin` |
| `wwwroot/css/admin.css` | Estilos `.admin-role-select` (tamano, estado disabled) |

**Verificacion end-to-end contra MySQL real (no mocks):**
- Navegador: login admin, `/users` muestra el selector de rol en cada tarjeta con el rol actual preseleccionado; la propia cuenta del admin aparece con el selector deshabilitado (confirmado via `getComputedStyle`/`disabled` en consola).
- `curl` directo contra `AdminAPI`: cambio de rol de un usuario de prueba (Company -> Candidate) devuelve `204`, `GET /users/{id}` confirma el cambio; revertido a `Company` con el mismo patron; intento de cambiar el rol propio del admin -> `409 "You cannot change your own role."`; rol invalido (`99`) -> `400 "Invalid role."`.

### Deuda tecnica #7: Unificar `AdminAuthService` con `AuthService`

**Archivos creados:** `Core/Interfaces/ITokenCryptoService.cs`, `Core/Services/TokenCryptoService.cs` — extraen `CreateJwtToken(claims, key, issuer, audience, expireMinutes)`, `GenerateRefreshToken()` y `HashToken(token)`, que antes eran metodos privados identicos en ambos servicios.

**Archivos modificados:** `AuthService.cs` y `AdminAuthService.cs` ahora reciben `ITokenCryptoService` por constructor y delegan la parte criptografica; cada uno conserva su propia logica de claims (`AuthService` agrega un claim `Role` por cada `UserRole` del usuario; `AdminAuthService` agrega un unico claim `Role=Admin`) y su propia configuracion `Jwt:Key/Issuer/Audience`. Registrado en `AddCoreServices` y `AddAdminCoreServices` (nunca se usan en el mismo host, sin doble registro).

**Verificacion:** login en `AdminAPI` (issuer `OpenToWork.Admin`) y registro+login+refresh-token en `OpenToWork.API` (issuer `OpenToWork.Portal`) contra MySQL real, confirmando que los JWT y refresh tokens se siguen generando y validando correctamente en ambos portales tras la unificacion.

### Deuda tecnica #8: Optimizar `AdminVacancyService`

`GetVacanciesAsync` cargaba `PT_Vacancies` y `PT_TempVacancies` completas en memoria (dos `ToListAsync()`) antes de combinar y paginar con LINQ-to-Objects. Se reescribe proyectando ambas tablas a `IQueryable<AdminVacancyDto>` con **exactamente el mismo conjunto de propiedades asignadas en ambos lados** (incluyendo `null` explicito en las que no aplican a cada tabla, ej. `ExpiresAt = null` en el lado de vacantes permanentes) y uniendolas con `.Concat()` antes del filtro de status, el `OrderByDescending` y el `Skip/Take`.

**Nota tecnica:** la primera version (sin igualar las propiedades asignadas en ambos lados) fallo en tiempo de ejecucion con `InvalidOperationException: Unable to translate set operations when both sides don't assign values to the same properties in the nominal type` — Pomelo/EF Core exige que ambos lados de un `Concat`/`Union` asignen el mismo conjunto de propiedades del tipo destino para poder traducirlo a `UNION ALL`.

**Verificacion:** `curl` contra `AdminAPI` real (MySQL) confirmo `200` con datos correctos en `GET /api/admin/vacancies` (paginado y filtrado por `status`), sin la excepcion de traduccion.

### Deuda tecnica #9: Mover `LocalStorageService`/`LanguageService` a `SharedUI`

`LocalStorageService` era identico byte a byte entre `AdminWEB` y `WEB` - se movio sin cambios a `OpenToWork.SharedUI.Services`.

`LanguageService` tenia la misma logica pero cargaba las traducciones de forma distinta (WEB itera un arreglo fijo de 9 secciones; AdminWEB carga un unico `admin.json`). Se unifico en una sola clase que recibe el arreglo de secciones por constructor - el nombre de cada seccion es tanto el nombre del archivo `.json` como el prefijo de flatten de sus claves, preservando el comportamiento exacto de ambos proyectos. Cada `Program.cs` registra su propia instancia via una lambda de fabrica con su arreglo de secciones.

Requirio agregar `<FrameworkReference Include="Microsoft.AspNetCore.App" />` a `OpenToWork.SharedUI.csproj` para acceder a `IWebHostEnvironment` (no disponible via el paquete `Components.Web` que ya referenciaba).

**Verificacion:** ambos portales muestran las traducciones ES/EN correctas tras la migracion (incluyendo las claves nuevas de gestion de roles); el portal principal (`/`) renderiza con sus 9 secciones sin cambios.

### Deuda tecnica #10: Centralizar el guard de autenticacion en `AdminLayout`

El chequeo `otwadmin-token` -> redirigir a `/login` estaba duplicado en 9 paginas (`CandidateProfile`, `Vacancies`, `Users`, `UserProfile`, `Skills`, `DashboardResults`, `Dashboard`, `AuditLog`, `Applications`). Se centraliza en `AdminLayout.OnInitializedAsync` y se elimina de esas 9 paginas.

**Hallazgo durante la implementacion:** las 4 paginas del Pipeline de Reclutamiento de Iluna (`Candidates/Index`, `Assigned`, `Pipeline`, `PipelineDetail`) **nunca tuvieron este guard** - eran accesibles sin sesion. Al vivir dentro del mismo `AdminLayout`, quedan protegidas automaticamente sin necesidad de tocarlas.

**Verificacion en navegador:** con `localStorage` limpio, `GET /candidates/pipeline` y `GET /users` redirigen a `/login` (antes, `/candidates/pipeline` renderizaba sin sesion). Con sesion iniciada, ambas paginas cargan con su logica intacta.

### Build y estado final

`dotnet build` sin errores en los 4 proyectos host (`API`, `AdminAPI`, `WEB`, `AdminWEB`) tras cada etapa. Todo verificado contra MySQL real en navegador y/o `curl`, no solo compilado.

**Pendiente (bloqueado por Fase 3, sin cambios):** verificaciones manuales (`PTVerification`) y revision de validaciones automaticas (`ValidationService`) - ninguna de las dos entidades existe todavia en el modelo de datos.
