# Bitácora de Cambios - OpenToWork

## Sesión: 3-4 Septiembre 2026

### IMPORTANTE - Nota sobre Migraciones

> **RECUERDA SIEMPRE:** Antes de ejecutar el proyecto después de un pull o merge, debes aplicar las migraciones de Entity Framework Core, ya que se han agregado nuevos campos a la base de datos.
>
> Comando para aplicar migraciones:
> ```bash
> dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.AdminAPI
> ```
>
> Migraciones nuevas en esta sesión:
> - `AddCandidateRecruitmentPreferences` - Preferencias de reclutamiento del candidato
> - `AddMigrationInfoAndDocuments` - Info migratoria, documentos y catálogo de tipos
> - `AddHasTransport` - Campo medio de transporte
> - `AddWorkAuthorizations` - Campo de autorizaciones de trabajo (multi-selección string)

---

### Cambios Realizados

#### 1. Info Migratoria del Candidato (AdminWEB - Pipeline Detail)
- **Campos nuevos en `PTCandidate`:** `Nationality`, `HasPassport`, `PassportNumber`, `WorkAuthorization` (int), `WorkAuthorizations` (string multi-selección), `HasTransport`
- **DTOs actualizados:** `RecruitmentDetailDto`, `UpdateMigrationInfoDto` con los nuevos campos
- **Servicio `RecruitmentService`:** Mapeo de campos en `GetDetailAsync`, `GetByUserIdAsync`, `UpdateMigrationInfoAsync`
- **UI `PipelineDetail.razor`:** Sección de info migratoria con edición inline
  - Nacionalidad (texto)
  - Tiene pasaporte (select Sí/No)
  - Permiso de trabajo / Autorización (checkboxes multi-selección):
    1. Ciudadano UE / EEE
    2. Permiso de residencia y trabajo vigente (no UE)
    3. Autorización de estancia por estudios CON permiso de trabajo
    4. Autorización de estancia por estudios SIN permiso de trabajo
    5. En trámite / pendiente de resolución
    6. Sin autorización para trabajar
  - **Auto-descarte:** Si se marca "Sin autorización para trabajar" (opción 6), el candidato se descarta automáticamente (stage 5)
  - **Auto-restauración:** Si se desmarca la opción 6 y el candidato estaba descartado, se restaura al stage anterior

#### 2. Medio de Transporte (AdminWEB - Pipeline Detail)
- Checkbox directo en "Información del candidato" con guardado instantáneo
- Removido de la sección de Info Migratoria (ya existe en info del candidato)

#### 3. Gestión de Documentos de Reclutamiento
- **Nuevas entidades:** `SYDocumentType` (catálogo), `PTRecruitmentDocument` (documentos por reclutamiento)
- **Seed data:** Documentos europeos comunes (DNI, Pasaporte, NIE, Certificado digital, etc.)
- **DTOs:** `DocumentTypeDto`, `RecruitmentDocumentDto`, `RequestDocumentDto`, `UpdateDocumentStatusDto`
- **Servicio:** Métodos `GetDocumentTypesAsync`, `GetRecruitmentDocumentsAsync`, `RequestDocumentAsync`, `UpdateDocumentStatusAsync`, `DeleteRecruitmentDocumentAsync`
- **API endpoints** en `RecruitmentController`

#### 4. Vinculación de Vacantes (AdminWEB - Pipeline Detail)
- **Validación Stage 4:** Al mover a "Listo a Entregar" valida que tenga vacante vinculada
- **UI de vinculación:** Modal con lista de vacantes activas, radio buttons, vincular/desvincular
- **Búsqueda inteligente:** Analiza el título del candidato (ej: "Técnico de Logística | Almacén | Distribución"), separa por keywords y hace match con título, descripción, requisitos y categoría de cada vacante
- **Buscador manual:** Filtra por título, empresa, ubicación, descripción o requisitos
- **DTOs:** `VacancyOptionDto` (con Description, Requirements, Category), `LinkVacancyDto`
- **Servicio:** `GetVacancyOptionsAsync`, `LinkVacancyAsync`, `RestoreCandidateAsync`
- **API endpoints:** `GET vacancies`, `PUT link-vacancy`, `POST restore`

#### 5. Restauración de Candidatos Descartados
- **Método `RestoreCandidateAsync`:** Revierte el descarte, restaura al stage anterior, elimina el registro de dismissal, registra log de etapa
- **Endpoint `POST {id}/restore`** en `RecruitmentController`
- **Método `RestoreCandidateAsync`** en `AdminAuthApiService`

#### 6. Portal del Candidato (OpenToWork.WEB) - Correcciones
- **Service Worker:** Bump de versión v2 → v4, agregados CSS faltantes (`bento-grid.css`, `home-v2.css`), fix de crash por cache stale
- **API BaseUrl:** Corregido de `localhost:5000` → `localhost:5100` en `appsettings.json`
- **Menú de navegación:**
  - Sin login: "Buscar empleo" y "Quiénes somos"
  - Con login: "Panel", "Mis Postulaciones" (o "Solicitudes" para empresas), "Mensajes"
  - Bottom nav móvil actualizada con la misma lógica
- **Ruta `/vacancies/create`:** Corregida a `/my-vacancies?action=create` (4 referencias en CompanyDashboard, Dashboard, VerifiedApplicants)
- **Botón crear vacante:** Agregado botón visible en header de `/my-vacancies`

#### 7. Diseño Login/Register (OpenToWork.WEB)
- **Formulario más amplio:** `max-width` de `420px` → `560px`
- **Padding reducido:** Más ligero para aprovechar el espacio
- **Video de fondo:** `v01.mp4` en la sección izquierda (brand) del `AuthLayout`
  - Video con `autoplay`, `muted`, `loop`, `playsinline`
  - Overlay semitransparente con degradado azul para legibilidad
  - Contenido (logo, título, tagline) por encima del video con z-index

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `PTCandidate.cs` | Campos: WorkAuthorizations, HasTransport |
| `PTCandidateRecruitment.cs` | VacancyId linking |
| `RecruitmentDtos.cs` | WorkAuthorizations, VacancyOptionDto, LinkVacancyDto |
| `DocumentDtos.cs` | UpdateMigrationInfoDto con WorkAuthorizations, HasTransport |
| `IRecruitmentService.cs` | GetVacancyOptionsAsync, LinkVacancyAsync, RestoreCandidateAsync |
| `RecruitmentService.cs` | Mapeo, update, vacancy options, link, restore |
| `RecruitmentController.cs` | Endpoints: vacancies, link-vacancy, restore, Stage 4 validation |
| `AdminAuthApiService.cs` | GetVacancyOptionsAsync, LinkVacancyAsync, RestoreCandidateAsync |
| `PipelineDetail.razor` | UI: checkboxes multi-selección, vacancy modal con buscador inteligente, auto-descarte/restauración |
| `MainLayout.razor` (WEB) | Menú condicional según login |
| `AuthLayout.razor` (WEB) | Video de fondo |
| `components.css` (WEB) | Auth form ampliado, video background styles |
| `App.razor` (WEB) | CSS version bumps |
| `sw.js` (WEB) | Cache v4, assets actualizados |
| `appsettings.json` (WEB) | BaseUrl corregido a 5100 |
| `MyVacancies.razor` (WEB) | Botón crear visible, query param action=create |
| `CompanyDashboard.razor` (WEB) | Fix ruta crear vacante |
| `Dashboard.razor` (WEB) | Fix ruta crear vacante |
| `VerifiedApplicants.razor` (WEB) | Fix ruta crear vacante |

### Migraciones EF Core Generadas

1. `20260904001300_AddCandidateRecruitmentPreferences`
2. `20260904021619_AddMigrationInfoAndDocuments`
3. `20260904031255_AddHasTransport`
4. `20260904042446_AddWorkAuthorizations`

---

## Sesión: 13 Septiembre 2026

### Refinamiento UI - Admin Pipeline (Kanban)

#### Problema
El pipeline de empresas y candidatos (`/companies/pipeline`, `/candidates/pipeline`) tenia scroll global en toda la pagina en lugar de solo en el kanban. Las stat cards eran muy anchas y no responsivas, y el kanban se desbordaba horizontalmente.

#### Cambios Realizados

**1. Scroll controlado (solo kanban scrollea)**
- `.admin-main:has(.admin-pipeline-kanban-wrapper)` → `height: 100vh`, `overflow: hidden` (antes `min-height: 100vh` permitia crecer sin limite)
- `.admin-content:has(.admin-pipeline-kanban-wrapper)` → `overflow: hidden`, `flex: 1`, `min-height: 0`, `display: flex`
- `.admin-content-inner:has(.admin-pipeline-kanban-wrapper)` → `max-width: 1400px`, `flex: 1`, `min-height: 0`, `display: flex`, `overflow: hidden`
- Header, stat-grid, filters → `flex-shrink: 0` (no se encojen)
- `.admin-pipeline-kanban-wrapper` → `flex: 1`, `min-height: 0`, `overflow: auto` (unico contenedor con scroll X e Y)

**2. Kanban wrapper introducido**
- Envuelto `.admin-pipeline-kanban` en nuevo `div.admin-pipeline-kanban-wrapper` en ambos `Companies/Pipeline.razor` y `Candidates/Pipeline.razor`

**3. Stat cards compactas (bento grid)**
- `padding: var(--space-sm) var(--space-md)` (antes `var(--space-lg)`)
- `gap: 2px` (antes `var(--space-xs)`)
- `border-radius: var(--radius-md)` (antes `var(--radius-lg)`)
- `font-size: 0.68rem` label, `1.5rem` value (antes `0.78rem` / `2rem`)
- Sin `min-height` (se adaptan al contenido)

**4. Stat grid responsivo**
- Desktop: `repeat(4, 1fr)` — 4 columnas
- Tablet (≤1024px): `repeat(2, 1fr)` — 2 columnas
- Movil (≤768px): `repeat(12, 1fr)` con `grid-column: span 12` — 1 card por fila, apiladas

**5. Kanban columnas**
- `min-width: 220px`, `max-width: 300px` (antes 140px/280px)
- `min-height: 900px` desktop, `500px` movil
- `flex: 1 1 220px` (se adaptan al espacio disponible)
- Movil: `flex: 0 0 180px`

**6. Cache-busting dinamico en CSS**
- Agregado `?v=@DateTime.Now.ToString("s")` a todos los `<link rel="stylesheet">` en `App.razor` de AdminWEB y WEB

**7. Responsive movil (≤768px)**
- `.admin-main` → `height: auto`, `overflow: visible` (no altura fija)
- `.admin-content` → `overflow-y: auto` (scroll normal de pagina)
- `.admin-content-inner` → `overflow: visible`

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `admin.css` | Scroll control, stat cards bento, kanban responsive, media queries |
| `Companies/Pipeline.razor` | Wrapper div alrededor del kanban |
| `Candidates/Pipeline.razor` | Wrapper div alrededor del kanban |
| `App.razor` (AdminWEB) | Cache-busting dinamico en CSS links |
| `App.razor` (WEB) | Cache-busting dinamico en CSS links |

---

## Sesión: 13 Septiembre 2026 (Cartera de Clientes y Pagos)

### Cartera de Clientes + Opciones de Pago para Recuperacion

#### Funcionalidad
Nueva seccion de cartera comercial (`/portfolio`) que muestra las empresas asignadas a cada comercial, con facturacion total, tramos pendientes y estado del pipeline. Vista centralizada de pagos (`/payments`) con filtro por estado y accion de marcar tramos como pagados.

#### Cambios Realizados

**1. API - PortfolioPaymentController**
- `GET /api/admin/portfolio` — agrupa empresas por comercial asignado, calcula facturacion y pendientes
- `GET /api/admin/payments` — lista paginada de tramos de pago con filtro por estado
- `POST /api/admin/payments/{trancheId}/mark-paid` — marca tramo como pagado
- Autorizacion: `RequireStaffRole(AdminStaffRole.Comercial)`

**2. DTOs (PortfolioPaymentDtos.cs)**
- `PortfolioSummaryDto` — resumen por comercial (empresas, facturacion, pendientes)
- `PortfolioCompanyDto` — empresa individual con vacantes, contratos y montos
- `PaymentListItemDto` — tramo de pago con contexto de contrato y empresa
- `PaymentListResultDto` — resultado paginado con totales

**3. UI AdminWEB**
- `Portfolio.razor` (`/portfolio`) — grid de cards por comercial con stats, modal de asignacion
- `PortfolioDetail.razor` (`/portfolio/{userId}`) — detalle de cartera por comercial
- `Payments.razor` (`/payments`) — tabla centralizada de tramos con filtro y accion de pago

### Archivos Nuevos

| Archivo | Descripcion |
|---------|-------------|
| `PortfolioPaymentController.cs` | API endpoints: portfolio, payments, mark-paid |
| `PortfolioPaymentDtos.cs` | DTOs de cartera y pagos |
| `Portfolio.razor` | Vista de cartera comercial con grid y modal asignacion |
| `PortfolioDetail.razor` | Detalle de cartera por comercial |
| `Payments.razor` | Vista centralizada de tramos de pago |

---

## Sesión: 20 Septiembre 2026

### IMPORTANTE - Nota sobre Migraciones

> **RECUERDA SIEMPRE:** Antes de ejecutar el proyecto después de un pull o merge, aplica las migraciones de Entity Framework Core — esta sesión agregó la tabla `PT_VerificationRequests`.
>
> ```bash
> dotnet ef database update -p src/OpenToWork.Models -s src/OpenToWork.Models
> ```
>
> Migraciones nuevas en esta sesión:
> - `VerificationRequests` - Tabla `PT_VerificationRequests` (solicitudes de verificación de identidad del candidato)

### IMPORTANTE - Datos de prueba

> Si la base de datos no tiene candidatos/empresas, usa las credenciales de `docs/credenciales-ejemplo.md` (seed `docs/seed-hosteleria.sql`):
> - **Candidatos**: `ana.martinez@gmail.com`, `luis.fernandez@hotmail.com`, `sofia.torres@outlook.com`, `javier.moreno@outlook.com` — password `Empresa123!`
> - **Empresas**: `rrhh@hotelsolcaribe.com`, `rrhh@lapaella.com`, `rrhh@cateringdelmar.com` — password `Empresa123!`
> - **Admin**: `admin@opentowork.com` / `Admin123!`

### Cambios Realizados

#### 1. Wizard de Verificación de Identidad (`/verification-request`)
- **Flujo completo** según spec legal (España): Paso 0 consentimiento con T&C/RGPD expandible → Paso 1 filtro "¿Nacionalidad española?" → Paso 2 situación administrativa → Ramas:
  - **Rama 1 (española)**: nombre, DNI con validación de letra de control (módulo 23), caducidad, anverso+reverso, teléfono +34, horario
  - **Rama 2 (UE/EEE/Suiza)**: nacionalidad (dropdown), NIE (X/Y/Z + 7 dígitos + letra), pasaporte + certificado registro UE
  - **Rama 3 (TIE)**: nacionalidad, NIE/TIE, tipo de permiso, caducidad, anverso+reverso
  - **Rama 4 (sin permiso)**: pantalla informativa Ley Orgánica 4/2000 + solo datos de contacto (minimización RGPD)
  - **Éxito**: check verde + referencia `#TD-XXXXXXXX`
- **Backend**: entidad `PTVerificationRequest` (con traza de consentimiento: IP, fecha, versión de términos), `VerificationRequestService`, `VerificationRequestsController` (`POST /api/verificationrequests`, `GET /api/verificationrequests/me`)
- **Uploads**: `InputFile` → base64 → API guarda en `uploads/verification/{id}/` (jpg/png/webp/pdf, máx 5MB)
- **DTOs**: `SubmitVerificationRequestDto`, `VerificationDocumentDto`, `VerificationRequestResultDto`
- Si el candidato ya tiene solicitud pendiente/en revisión, el wizard muestra su referencia directamente

#### 2. Dashboard candidato (`/dashboard`)
- Avatar y nombre clickeables → `/profile` (eliminado botón "Ver perfil")
- Botón **"Solicitar verificación"** → `/verification-request`
- Botón **"Mejorar plan"** → `/plans`
- i18n: `dashboard.actions.requestVerification` + `dashboard.actions.upgradePlan` (es/en)

#### 3. Perfil de empresa (`/company-profile`)
- **API**: `GET/PUT /api/companies/me` + DTOs `MyCompanyProfileDto`/`UpdateMyCompanyProfileDto` + `CompanyService.GetMyCompanyAsync`/`UpdateMyCompanyAsync`
- **Página editable** con hero: logo grande, nombre, badge "Empresa verificada" prominente, chips de industria/ubicación, link a perfil público
- **Fix**: `/profile` redirigía empresas al login (API devolvía null) — ahora `MainLayout` linkea a `/company-profile` para rol empresa y `Profile.razor` redirige empresas

#### 4. Dashboard empresa (`/company-dashboard`)
- Botón **"Mejorar plan"** → `/plans`
- **Página `/plans`**: 3 tiers (Gratis, Pro, Premium) con estilos propios

#### 5. Empresas públicas
- `/companies` (listado) y `/companies/{id}` (detalle con vacantes activas)

#### 6. Modal de reconexión Blazor rediseñado
- Pulso blanco centrado + "Reconectando..." sobre backdrop oscuro con blur (reemplaza template default en inglés)
- Cards en español para estados failed/paused/resume-failed
- **Nota técnica**: los estilos están en `base.css` (no en `ReconnectModal.razor.css`) porque `App.razor` no enlaza el bundle de CSS isolation `OpenToWork.WEB.styles.css`

### Archivos Nuevos

| Archivo | Descripción |
|---------|-------------|
| `PTVerificationRequest.cs` | Entidad solicitud de verificación |
| `VerificationRequestDtos.cs` | DTOs submit/documento/resultado |
| `IVerificationRequestService.cs` + `VerificationRequestService.cs` | Lógica de solicitudes + guardado de docs |
| `VerificationRequestsController.cs` | API endpoints de verificación |
| `VerificationRequest.razor` | Wizard de verificación (6 pasos/ramas) |
| `CompanyProfile.razor` | Perfil de empresa editable con hero |
| `CompaniesController.cs` + `ICompanyService.cs` + `CompanyService.cs` | API/servicio de empresas |
| `Plans.razor` | Página de planes |
| `Companies.razor` + `CompanyDetail.razor` | Empresas públicas |
| `20260920231100_VerificationRequests.cs` | Migración (limpiada: sin churn de seeds) |

---

## Sesión: 21 Septiembre 2026

### IMPORTANTE - Migraciones

> Migraciones nuevas en esta sesión (todas limpiadas a mano: el scaffold automático de `dotnet ef migrations add` sigue arrastrando un bug preexistente en `AppDbContext.SeedWizardSteps`/`SeedDocumentTypes` — usan `Guid.NewGuid()` en el seed, así que cada `migrations add` genera IDs nuevos y EF quiere borrar/reinsertar esas filas sin motivo. Se recorta manualmente cada vez para que la migración solo toque lo relevante):
> - `PlanFeaturesAndFeatured` — columnas `Features`/`IsFeatured` en `PT_Plans`
> - `CandidatePlanTierAndPlanAudience` — columna `Audience` en `PT_Plans`, columna `PlanTier` en `PT_Candidates`, seed de 3 planes de candidato (Free/Basic/Premium)
> - `PlanPricePrecision` — `PT_Plans.Price` de `decimal(65,30)` (default de Pomelo, sin precisión declarada) a `decimal(10,2)`

### Cambios Realizados

#### 1. Conexión de `/plans` (WEB) a la base de datos real
- Antes: `Plans.razor` tenía 3 tarjetas (Gratis/Pro/Premium) con HTML fijo, sin relación con `PT_Plans`.
- Ahora: endpoint público `GET /api/plans` (nuevo `PlansController` en `OpenToWork.API`) sirve el catálogo real; `Plans.razor` itera sobre la respuesta.
- `PTPlan` ganó `IsFeatured` (bool) y `Features` (string, un beneficio por línea) para que el admin controle qué se destaca y qué lista de beneficios se muestra — antes esos datos no existían en el modelo.
- CRUD de `/settings/plans` (AdminWEB) actualizado con los campos nuevos.

#### 2. Plan de mejora para candidatos (Free/Basic/Premium) — nuevo
- **Decisión de Darwin (21-Sep):** retoma la idea documentada el 18-Sep ("Plan de Prioridad para Candidatos") pero con 3 niveles en vez de uno solo, mismo patrón que empresas.
  - **Free**: lo que ya tiene cualquier candidato.
  - **Basic** (5.99€, destacado): + prioridad en el matching + "Verificación por Trato Directo".
  - **Premium** (9.99€): todo Basic + "Asesoría en construcción de CV con un especialista".
  - Los dos beneficios de Basic/Premium que dependen de un humano (verificación, asesoría de CV) son compromisos operativos — el código solo los muestra como texto del plan, no dispara ninguna automatización nueva.
- **Modelo**: `PTPlan.Audience` (enum `Company`/`Candidate`) distingue el catálogo por audiencia; `PTCandidate.PlanTier` (enum `Free`/`Basic`/`Premium`, default `Free`) guarda el nivel del candidato — asignado manualmente por un admin desde `/candidates/profile/{id}` (no hay checkout, no hay pasarela de pagos integrada).
- **Prioridad real en matching**: `CompatibilityService.GenerateShortlist`/`GetNonApplicantMatchesAsync` ordenan primero por `HasPriorityPlan` (Basic/Premium), luego por `IsVerifiedTD`, luego por `MatchPercentage` — pero **solo si el flag de candidatos está encendido**; si está apagado, el orden es idéntico al de antes de este cambio.
- **Flags independientes por audiencia** (pedido de Darwin tras ver que solo había un flag): `feature_company_plans_enabled` (encendido por defecto — los planes de empresa ya eran visibles antes de este flag) y `feature_candidate_priority_plan_enabled` (apagado por defecto). Se pueden prender/apagar por separado desde `/settings/plans` (dos checkboxes independientes). Ambos viven en `SY_SystemConfig`, categoría `Features`.
- El botón "Mejorar plan" de `/dashboard` (candidato) y `/company-dashboard` (empresa) ahora solo se muestra si el flag de su audiencia está encendido — antes el de candidato apuntaba (sin ningún control) a los planes de empresa por error.

#### 3. Fix: decimales del precio de los planes
`PT_Plans.Price` no tenía precisión declarada en el modelo, así que Pomelo/MySQL usaba el default `decimal(65,30)` — cada precio arrastraba hasta 30 ceros decimales (visible en el JSON crudo de la API y en el campo de edición del formulario admin, aunque la tabla y `/plans` público ya lo recortaban con formato). Se agregó `HasPrecision(10, 2)` en `AppDbContext` y se migró la columna; ahora `Price` es `decimal(10,2)` real en MySQL, no hay forma de que vuelvan a aparecer decimales de más en ningún punto.

#### 4. UI del formulario de Planes (AdminWEB) rediseñada
El formulario original metía Nombre/Descripción/Precio/Moneda/Orden/Beneficios en una sola fila flex (`admin-inline-form`) — el textarea de Beneficios quedaba apretado al mismo ancho angosto que los demás campos. Se rediseñó con el mismo patrón de tarjetas (`admin-chart-grid`/`admin-chart-card`) que usa `/settings/company-profile`: una tarjeta "Datos del plan" y otra "Presentación" con el textarea a ancho completo.

#### 5. Video de fondo en el login de AdminWEB
`LoginLayout.razor` ahora tiene el mismo video de fondo (`v01.mp4`) + overlay degradado azul que ya usaba el login/registro de WEB (`AuthLayout.razor`). De paso se corrigió un bug preexistente: `components.css` tenía una regla `.auth-container` duplicada con fondo opaco que tapaba el video — se sobreescribió en `admin.css` (que carga después).

#### 6. Autofocus en el campo de correo del login (AdminWEB)
El atributo HTML `autofocus` no alcanzaba porque Blazor mueve el foco al `<h1>` de la página después de cada navegación (`<FocusOnNavigate Selector="h1" />` en `Routes.razor`, para accesibilidad). Se agregó `focus-helper.js` + una llamada JS en `OnAfterRenderAsync(firstRender)` que gana esa carrera.

### Archivos Nuevos

| Archivo | Descripción |
|---------|-------------|
| `PlansController.cs` (OpenToWork.API) | Endpoint público `GET /api/plans` (con `audience`) y `GET /api/plans/{company,candidate}-enabled` |
| `PlanAudience.cs` + `CandidatePlanTier.cs` | Enums nuevos en `OpenToWork.Shared.Enums` |
| `focus-helper.js` (AdminWEB) | Helper JS para enfocar un input por id tras el render |
| `20260921050843_PlanFeaturesAndFeatured.cs` | Migración: `Features`/`IsFeatured` en `PT_Plans` |
| `20260921224421_CandidatePlanTierAndPlanAudience.cs` | Migración: `Audience`, `PlanTier`, seed de planes de candidato |
| `20260921231018_PlanPricePrecision.cs` | Migración: `Price` a `decimal(10,2)` |

### Pendiente

- **Terminar el flujo del candidato en el panel administrativo** (pedido de Darwin, 21-Sep) — sin alcance definido todavía, queda para la próxima sesión.

> **Nota**: el commit también incluye cambios pendientes de AdminAPI/AdminWEB de la sesión anterior (perfil de candidato admin, detalle de empresa, etc.).

---

## Sesión: 22-24 Septiembre 2026

### IMPORTANTE - Migraciones

> - `PlanSubscriptionFields` — `PlanExpiresAt` + `StripeCustomerId`/`StripeSubscriptionId` en `PT_Candidates` y `PT_Companies`, `PlanTier` en `PT_Companies`. **Rellena `PlanExpiresAt` (+1 mes) para candidatos que ya tenian plan de pago**; si la migracion se aplico antes de ese arreglo (commit `292f6ad`), correr a mano:
>   `UPDATE PT_Candidates SET PlanExpiresAt = DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 1 MONTH) WHERE PlanTier <> 0 AND PlanExpiresAt IS NULL;`
> - `CandidatePlacementRelease` — `PlacementEndedAt`/`PlacementEndReason`/`PlacementEndNotes`/`PlacementEndedByUserId` en `PT_CandidateDeliveries` y `PT_Negotiations` ("Liberar candidato"). Recortada a mano (ruido de seed de `SY_DocumentTypes`/`SY_WizardSteps`/`PT_Plans`).
> - "Colocado" y la respuesta de la empresa desde el admin **no agregan migraciones**.

### Cambios Realizados

#### 1. Vencimiento de planes (candidato y empresa) — commit `292f6ad`
- `PlanCalculator`: un plan asignado a mano vale 1 mes; si vencio se ignora (misma idea que `WarrantyCalculator`, sin BackgroundService). La prioridad en el ranking de `CompatibilityService` solo cuenta si el plan esta vigente.
- Selector de plan de empresa (None/Basic/Premium/Platinum) en `Companies/Detail.razor` (pantalla de Iluna) + badge vigente/vencido en empresa y candidato. El plan de empresa todavia no desbloquea nada — solo se registra.
- Campos de Stripe listos para cuando exista el checkout (Fase 7).

#### 2. Respuesta de la empresa registrada desde el admin — commit `60e9c86` (inicio de "Terminar el flujo del candidato en el panel administrativo")
- Antes solo la empresa, desde su portal, podia marcar una entrega como Interesado/Contratado/Descartado. Si respondia por telefono o WhatsApp el flujo quedaba trabado (contratacion, incorporacion, garantia y cierre exigen Contratado).
- `PUT api/admin/recruitment-deliveries/{id}/company-response` + boton "Registrar respuesta de la empresa" en la tarjeta de cada entrega (`Candidates/PipelineDetail.razor`), con nota. Queda en el audit log (`Recruitment.RecordCompanyResponse`, `source=admin`).
- No se puede cambiar con el proceso cerrado ni sacar de Contratado si ya hay fecha de contratacion/incorporacion (misma regla en servidor y UI).

#### 3. Estado "Colocado" + estado de entregas visible
- `CandidatePlacementHelper` (regla unica, calculada en vivo, sin columna nueva): un candidato esta **Colocado** si tiene una entrega en Contratado o gano una negociacion Cerrada, sin reposicion de garantia activa sobre ella (una Cancelada no cuenta). Si deja el puesto vuelve a estar disponible.
- `DeliveryService.DeliverCandidateAsync` rechaza entregar a un candidato Colocado.
- Pipeline (`/candidates/pipeline`): columna virtual "Colocado" (sale de "Verificado"), tarjeta de estadistica "Colocados", y cada tarjeta muestra su estado de entrega.
- Componente `PlacementBadge.razor` ("Colocado en X" / "Entregado · X (N entregas)" / "Descartado · X") en el pipeline, la consola de candidatos, el perfil y el detalle del pipeline (donde ademas se desactiva "Entregar a empresa").
- Nota: con los datos de prueba actuales, algunos candidatos (p. ej. Donald, Juan Perez) salen Colocados por negociaciones Cerradas de pruebas anteriores — la regla es correcta, son datos de test.
- **Un candidato Colocado no esta disponible para otra plaza** (`CandidatePlacementHelper.PlacedCandidateIds`, filtro en SQL): no puede postularse desde su portal (ve el motivo en la vacante), no se puede presentar en una negociacion ni elegir como ganador, no aparece en el Ranking por Compatibilidad, en "Cumplen sin postularse", en la busqueda de candidatos (`/candidate-search`) ni en el calculo de matches de vacantes nuevas. Excepcion: la vacante donde fue contratado no cuenta como "otra plaza".
- **Liberar candidato** (detalle del pipeline, visible solo si esta Colocado): cuando deja el puesto **fuera de garantia** (renuncia, despido, fin de contrato, otro + notas) se registra el fin de todas sus colocaciones activas (entregas y negociaciones) y vuelve a estar disponible. Dentro de la garantia se rechaza: esa salida se gestiona con una reposicion de garantia. Queda en el audit log (`Recruitment.ReleaseCandidate`) y la entrega muestra "Dejo el puesto el X · motivo".

### Pendiente

- Resto de "Terminar el flujo del candidato": que el candidato vea su proceso en su portal, que vea su plan/vencimiento, avisos por correo al candidato (entregado/contratado), documentos pedidos por el reclutador desde el portal del candidato.

---

## Sesión: 25 Septiembre 2026 (recorrido de Darwin por el sistema)

### IMPORTANTE - Migraciones

> Aplicar a mano (`dotnet ef database update ...`). Todas recortadas del ruido de seed de siempre (`SY_DocumentTypes`/`SY_WizardSteps`/`PT_Plans`).
> - `DeliveryRejectionReason` — `PT_CandidateDeliveries.RejectionReason`
> - `ExperienceLevelYearRanges` — solo datos: vacantes a la escala nueva de experiencia (Junior→1-3, Mid→3-5, Senior/Lead→Mas de 5)
> - `ContractLinePositions` — `PT_ContractVacancies.Positions`/`LineTotal` (lineas existentes: 1 posicion, `LineTotal = FinalPrice`, no altera importes historicos)
> - `ContractVersions` — `PT_VacancyContracts.Version` + tabla `PT_ContractRevisions`
> - `ActivateWonProspects` — solo datos: empresas en Cerrado Ganado que seguian como Prospecto → Activa
> - `Messaging` — tablas `PT_Conversations` y `PT_Messages`

### Cambios Realizados

#### 1. Historial de entregas por candidato + candidato "quemado"
- Motivo estructurado al descartar una entrega (`DeliveryRejectionReason`), obligatorio desde el admin y desde el API de la empresa.
- Seccion "Historial de entregas" en el perfil del candidato: empresas distintas, contratado, rechazos, sin respuesta, rechazos por motivo y tabla de entregas.
- "Quemado" = 3 o mas rechazos sin ninguna contratacion (`CandidatePlacementHelper.IsBurned`), visible en pipeline, consola, perfil y detalle.

#### 2. Alta de empresa y vacantes (pantallas de Iluna)
- Mapa: "Usar ubicacion" espera la geocodificacion (antes podia dejar la direccion vacia), usa el marcador si no se toco el mapa, muestra la direccion elegida y agrega el codigo postal.
- La ubicacion de cada vacante nueva arranca con la direccion + ciudad de la empresa.
- Experiencia por rangos (Sin experiencia / Menos de 1 / 1-3 / 3-5 / Mas de 5); se quito "Anos de experiencia" del formulario.
- La revision del alta muestra direccion y descripcion.

#### 3. Contrato de vacantes
- **Bug**: con tarifa por posicion se cobraba una sola posicion. Ahora total = precio unitario x posiciones (clausula 6.2). "Posiciones a cubrir" editable desde el contrato (actualiza la vacante). "N.º objetivo de candidatos" pasa a "Candidatos a presentar (objetivo)".
- Motivo obligatorio en precio fijo (manual).
- Borrador con marca de agua "BORRADOR - no valido para firma" + botones "Editar borrador" / "Emitir version oficial".
- "Abrir nueva version" (contrato Enviado/Aceptado → Borrador v+1, con motivo; copia completa de la version anterior en `PT_ContractRevisions`). Al reaceptar: tramos pendientes recalculados; diferencia sobre lo ya cobrado en un tramo "Ajuste" (positivo = falta cobrar, negativo = saldo a favor). Decision de Darwin.

#### 4. Empresas
- Cerrado Ganado → la empresa pasa de Prospecto a Activa (`CompanyCrmService.MoveStageAsync`, unico punto para cambio manual y aceptacion de contrato).
- "Dar acceso al portal" (`CompanyPortalAccessService`): crea el usuario de la empresa con su correo de contacto, lo vincula a la empresa existente y genera un enlace de activacion (7 dias, solo se guarda el hash; por correo si el SMTP esta activo). Pagina nueva del portal `/reset-password`.

#### 5. Mensajeria real (portal ↔ Trato Directo)
- `/messages` del portal devolvia 4 conversaciones inventadas fijas. Ahora `MessagingService` + `PT_Conversations`/`PT_Messages`, **solo usuario del portal ↔ equipo de Trato Directo** (Embudo Ciego; decision de Darwin). El portal ve siempre "Trato Directo".
- Admin: bandeja `/messages` (sin leer, busqueda, hilo con "leido", respuesta), "Mensajes" en el menu con contador, "Enviar mensaje" desde la ficha del candidato y de la empresa. Aviso por correo al usuario si el SMTP esta activo.

#### 6. Portal de empresa
- `/dashboard` redirige a `/company-dashboard` si el usuario es empresa (y viceversa); enlace del pie de pagina y fin del registro corregidos.
- Pagina `/my-vacancies/{id}/edit` (el boton "Editar vacante" llevaba a una ruta inexistente). Con contrato Enviado/Aceptado la empresa edita solo descripcion, requisitos, experiencia e ingles; lo demas y el cierre se piden por Mensajes ("Solicitar cambio" / "Solicitar cierre"). Decision de Darwin.
- **Seguridad**: editar/eliminar/publicar/cerrar vacantes en el API publico no comprobaba la empresa duena → cualquier usuario del portal podia modificar o borrar vacantes ajenas. Corregido con `OwnsVacancyAsync` (403).

### Pendiente
- Vista del proceso y del plan en el portal del candidato; avisos por correo al candidato (entregado/contratado).
- Portal de empresa: el saludo usa la parte del correo en vez del nombre de la empresa; la campana muestra alertas pensadas para candidatos.
- La interfaz de negociaciones ya no esta en `Vacancies.razor` (quedo solo el code-behind).

---

## Sesión: 25 Septiembre 2026, tarde (revisión de seguridad y páginas legales)

### IMPORTANTE - Migraciones y CV

> - `PrivacyContactEmail` — solo datos: clave `company_privacy_email` en `SY_SystemConfig`.
> - **Los CV salieron del repositorio** (el repo de GitHub es público y tenía 23 CV de personas reales). Ahora se guardan en `storage/cv` (ignorado por git). Antes del pull, copiar los PDF de `wwwroot/uploads/cv` a `storage/cv`. Pendiente coordinar: repo privado + purga del historial.

### Cambios Realizados

#### 1. Seguridad del API público
- `GoogleTokenValidator`: valida firma (JWKS de Google, caché 6 h), emisor, audiencia (`GoogleOAuth:ClientId`) y vigencia. Sin `ClientId` el login con Google queda desactivado. Solo vincula una cuenta existente si el correo está verificado.
- `ProfileService`: las 6 operaciones de editar/borrar secciones del perfil comprueban el dueño.
- `GetCandidateByIdAsync(candidateId, viewerUserId)`: dueño completo; empresa solo si le fue entregado, sin identificación, fecha de nacimiento ni dirección; resto 404.
- `ICvStorage`/`CvStorage`: CV fuera de `wwwroot`, descarga por endpoints con permiso; el nombre de archivo debe empezar por `cv_{userId}_` (bloquea rutas manipuladas).

#### 2. Portal
- `.home-vacancies-grid` con `auto-fill, minmax(260px, 1fr)` (la ficha de empresa obligaba a hacer scroll).
- Quitada la sección "Empresas que confían en nosotros" de la página de inicio.
- `/privacy`: política de privacidad genérica (12 apartados); contacto = correo de privacidad o, si falta, la dirección postal.
- `/terms`: términos y condiciones genéricos (13 apartados), contacto info@tratodirecto.es. Coherentes con la cláusula 12 del contrato (no contratación directa).
- Ambos textos deben revisarlos un asesor legal.

#### 3. Admin
- Lista de vacantes: días desde la firma del contrato (primer `AcceptContract` del registro de auditoría) hasta hoy o hasta el cierre, en hora local, y días hábiles contra el objetivo de cobertura.
- Datos de la Empresa: campo "Correo de privacidad" (endpoint público `api/legal/identity`, sin DNI del representante).

### Pendiente
- Repo privado y purga de CV del historial (coordinación Darwin–Ivan).
- Revisión de seguridad puntos 5 (búsqueda de candidatos abierta a cualquier usuario) y 6 (controles de rol: candidato creando vacantes, empresa en `/candidates/me`).
- Decidir si la empresa con candidato entregado debe ver su teléfono.
- Aviso legal (LSSI) con los datos del titular, si se quiere publicar.
