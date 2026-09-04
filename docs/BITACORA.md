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
