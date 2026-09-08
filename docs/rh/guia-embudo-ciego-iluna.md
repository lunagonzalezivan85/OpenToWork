# Guía de Implementación: Embudo Ciego (Entrega de Personal Verificado)

> **Para:** Iluna  
> **De:** RH  
> **Fecha:** 08-Sep-2026  
> **Objetivo:** Implementar el modelo de negocio de Trato Directo: la empresa ve conteos de postulantes (nunca identidad), y solo ve el personal que TD le entrega ya verificado.

---

## Resumen del flujo a implementar

```
Empresa publica vacante
        │
Postulantes aplican ──► Empresa ve SOLO conteos en la vacante:
        │               • "12 postulantes" (total)
        │               • "3 en verificación" (en proceso en pipeline TD)
        │
Admin (TD) revisa pipeline: Investigación → Evaluación → Entrevista → Listo a Entregar
        │
Admin ENTREGA candidato verificado a la empresa (acción nueva)
        │
NUEVA VISTA empresa "Personal Verificado": solo candidatos entregados
        │
Empresa decide: Interesado / Contratado / No encaja
```

---

## Paso 1: Entidad de Entrega + Enum

### Enum nuevo — `src/OpenToWork.Shared/Enums/DeliveryStatus.cs`

```csharp
namespace OpenToWork.Shared.Enums;

public enum DeliveryStatus
{
    Delivered = 0,          // Entregado por TD, empresa aún no lo ve/revisa
    ViewedByCompany = 1,    // La empresa abrió el perfil
    Interested = 2,         // La empresa marcó interés
    Hired = 3,              // Contratado
    RejectedByCompany = 4   // La empresa descartó
}
```

### Entidad nueva — `src/OpenToWork.Models/Entities/PTCandidateDelivery.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCandidateDelivery : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    [Required]
    public Guid DeliveredByUserId { get; set; }

    [ForeignKey("DeliveredByUserId")]
    public virtual SCUser DeliveredByUser { get; set; } = null!;

    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    public int Status { get; set; } = (int)DeliveryStatus.Delivered;

    [MaxLength(1000)]
    public string? AdminNote { get; set; }      // Nota de presentación del admin

    [MaxLength(1000)]
    public string? CompanyFeedback { get; set; } // Feedback si rechaza

    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
```

### Registro en `AppDbContext.cs`

```csharp
public DbSet<PTCandidateDelivery> PT_CandidateDeliveries => Set<PTCandidateDelivery>();
```

Y en `OnModelCreating`:

```csharp
modelBuilder.Entity<PTCandidateDelivery>(e =>
{
    e.ToTable("PT_CandidateDeliveries");
    e.HasIndex(d => new { d.PT_CompanyId, d.IsDeleted });
    e.HasIndex(d => new { d.PT_VacancyId, d.IsDeleted });
    e.HasIndex(d => new { d.PT_CandidateRecruitmentId, d.IsDeleted });
    e.HasIndex(d => new { d.Status, d.IsDeleted });
});
```

### Migración

```bash
dotnet ef migrations add CandidateDeliveries --project src/OpenToWork.Models --startup-project src/OpenToWork.AdminAPI
dotnet ef database update --project src/OpenToWork.Models --startup-project src/OpenToWork.AdminAPI
```

---

## Paso 2: Servicio de Entregas (Core)

### Interfaz — `src/OpenToWork.Core/Interfaces/IDeliveryService.cs`

```csharp
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IDeliveryService
{
    // Admin
    Task<DeliveryDto?> DeliverCandidateAsync(DeliverCandidateDto dto, Guid adminId, string? ipAddress);
    Task<List<DeliveryDto>> GetDeliveriesByRecruitmentAsync(Guid recruitmentId);

    // Empresa
    Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid companyUserId);
    Task<VacancyApplicantSummaryDto> GetVacancySummaryAsync(Guid vacancyId, Guid companyUserId);
    Task<DeliveryDto?> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback, Guid companyUserId);
}
```

### DTOs — `src/OpenToWork.Shared/DTOs/DeliveryDtos.cs`

```csharp
namespace OpenToWork.Shared.DTOs;

public class DeliverCandidateDto
{
    public Guid RecruitmentId { get; set; }
    public Guid VacancyId { get; set; }
    public string? AdminNote { get; set; }
}

public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateTitle { get; set; }
    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public string? CompanyFeedback { get; set; }
    public DateTime DeliveredAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int OverallScore { get; set; }
    public int ProfileCompletionPercentage { get; set; }
    public bool IsVerifiedTD { get; set; }
}

public class VacancyApplicantSummaryDto
{
    public Guid VacancyId { get; set; }
    public int Total { get; set; }
    public int InVerification { get; set; }
    public int Delivered { get; set; }
}

public class RespondDeliveryDto
{
    public int Status { get; set; }      // 2=Interested, 3=Hired, 4=Rejected
    public string? Feedback { get; set; }
}
```

### Implementación — `src/OpenToWork.Core/Services/DeliveryService.cs`

Puntos clave de la lógica:

1. **`DeliverCandidateAsync` (admin):**
   - Cargar `PTCandidateRecruitment` por Id, validar `!IsDeleted` y `CurrentStage == (int)RecruitmentStage.ReadyToDeliver`.
   - **Requisito de verificación:** llamar `IVerificationStatusService.GetVerificationStatusAsync(candidateId)` y validar `IsVerifiedTD == true`. Si no, lanzar error "El candidato debe estar Verificado TD antes de ser entregado".
   - Cargar la vacante y su empresa (`PT_Vacancies.Include(v => v.Company)`), validar que la vacante esté activa.
   - Validar que no exista ya un delivery activo para el mismo `PT_CandidateRecruitmentId` + `PT_VacancyId`.
   - Crear `PTCandidateDelivery` con `Status = Delivered`, `DeliveredAt = UtcNow`.
   - Auditoría: `_auditLog.LogAsync(adminId, "Recruitment.Deliver", "PTCandidateDelivery", deliveryId, ...)`.

2. **`GetMyDeliveriesAsync` (empresa):**
   - Filtrar `d.Company.SCUserId == companyUserId && !d.IsDeleted`.
   - Incluir Candidate, Vacancy.
   - Para cada delivery, calcular `IsVerifiedTD`, `OverallScore`, `ProfileCompletionPercentage` con `IVerificationStatusService`.
   - Ordenar por `DeliveredAt` desc.

3. **`GetVacancySummaryAsync` (empresa):**
   - Total: `PT_Applications` de la vacante (`!IsDeleted`).
   - InVerification: total menos los que tienen delivery activo en esa vacante.
   - Delivered: `PT_CandidateDeliveries` de la vacante (`!IsDeleted`).
   - Validar que la vacante pertenece a la empresa (`vacancy.Company.SCUserId == companyUserId`), si no, devolver vacío.

4. **`RespondToDeliveryAsync` (empresa):**
   - Cargar delivery, validar `d.Company.SCUserId == companyUserId`.
   - Solo permitir status 2 (Interested), 3 (Hired), 4 (RejectedByCompany).
   - Si pasa de Delivered → cualquier acción, setear `ViewedAt` si es null.
   - Setear `Status`, `CompanyFeedback`, `RespondedAt = UtcNow`.
   - Auditoría.

### Registro en DI — `src/OpenToWork.Core/Extensions/ServiceCollectionExtensions.cs`

```csharp
services.AddScoped<IDeliveryService, DeliveryService>();
```

---

## Paso 3: Endpoints API

### A) AdminAPI — `src/OpenToWork.AdminAPI/Controllers/DeliveriesController.cs` (nuevo)

```csharp
[Route("api/admin/recruitment-deliveries")]
[RequireStaffRole(AdminStaffRole.Reclutador)]
public class DeliveriesController : AdminControllerBase
{
    [HttpPost]                    // Entregar candidato
    public async Task<IActionResult> Deliver([FromBody] DeliverCandidateDto dto)
        => Ok(await _deliveryService.DeliverCandidateAsync(dto, AdminId, ClientIp));

    [HttpGet("recruitment/{recruitmentId}")]   // Historial de entregas del candidato
    public async Task<IActionResult> ByRecruitment(Guid recruitmentId)
        => Ok(await _deliveryService.GetDeliveriesByRecruitmentAsync(recruitmentId));
}
```

### B) API candidatos/empresa — `src/OpenToWork.API/Controllers/DeliveriesController.cs` (nuevo)

```csharp
[Route("api/[controller]")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    [HttpGet("my")]               // Personal verificado de la empresa
    public async Task<IActionResult> My() { /* userId de token → GetMyDeliveriesAsync */ }

    [HttpGet("vacancy-summary/{vacancyId}")]   // Conteos de la vacante
    public async Task<IActionResult> VacancySummary(Guid vacancyId) { /* → GetVacancySummaryAsync */ }

    [HttpPut("{id}/respond")]     // Interesado / Contratado / No encaja
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondDeliveryDto dto) { /* → RespondToDeliveryAsync */ }
}
```

### C) Modificar `ApplicationsController` (OpenToWork.API)

1. **`GetByVacancy` (GET api/applications/vacancy/{vacancyId}):** dejar de exponer identidad a la empresa. Dos opciones:
   - **Opción simple (recomendada):** cambiar el retorno a `VacancyApplicantSummaryDto` (solo conteos). El nombre del endpoint puede mantenerse; el frontend se actualiza en el Paso 5.
   - **Opción alternativa:** crear endpoint nuevo `GET api/applications/vacancy/{vacancyId}/summary` y deprecar el anterior.

2. **`UpdateStatus` (PUT api/applications/{id}/status):** eliminar el endpoint o restringirlo a rol admin. La empresa ya no cambia estados de postulaciones; usa `PUT api/deliveries/{id}/respond`.

---

## Paso 4: UI Admin — Acción "Entregar a empresa"

**Archivo:** `src/OpenToWork.AdminWEB/Components/Pages/Candidates/PipelineDetail.razor`

En la sección de la etapa **Listo a Entregar (stage 4)**:

1. Agregar botón **"Entregar a empresa"** (solo visible si `Detail.CurrentStage == 4` y `Detail.VacancyId.HasValue`).
2. Modal de entrega:
   - Muestra la vacante vinculada (`Detail.VacancyTitle`) — la entrega va a la empresa de esa vacante.
   - Textarea opcional "Nota de presentación" (`AdminNote`).
   - Si el candidato NO está Verificado TD: mostrar el botón deshabilitado con tooltip "Requiere Verificado TD".
3. Al confirmar: llamar `POST api/admin/recruitment-deliveries`, mostrar toast de éxito, recargar sección de entregas.
4. Agregar sección "Entregas" que liste los deliveries existentes del candidato (vacante, fecha, estado, feedback de la empresa si existe).

**Cliente HTTP — `AdminAuthApiService.cs`:**

```csharp
public async Task<DeliveryDto?> DeliverCandidateAsync(Guid recruitmentId, Guid vacancyId, string? note)
{
    await SetAuthHeaderAsync();
    var response = await _httpClient.PostAsJsonAsync("api/admin/recruitment-deliveries",
        new DeliverCandidateDto { RecruitmentId = recruitmentId, VacancyId = vacancyId, AdminNote = note });
    return response.IsSuccessStatusCode
        ? await response.Content.ReadFromJsonAsync<DeliveryDto>() : null;
}
```

---

## Paso 5: UI Empresa — Nueva vista "Personal Verificado"

**Archivo:** `src/OpenToWork.WEB/Components/Pages/VerifiedApplicants.razor` (reescribir)

### Vista 1 — Lista de vacantes (sin cambios de concepto):
- Cada card muestra: título, estado, vistas y **dos conteos**: `total postulantes` y `X en verificación` (del endpoint summary).
- **Ya no** se puede hacer clic para "ver postulantes". En su lugar, CTA: "Ver personal verificado" → navega a la vista de entregados filtrada por vacante.
- Si `delivered == 0`: mostrar texto "Aún no hay personal verificado entregado para esta vacante".

### Vista 2 — Personal Verificado (nueva):
- Endpoint: `GET api/deliveries/my` (opcionalmente con filtro `?vacancyId=`).
- Cards con: nombre, título, badge "★ Verificado TD", score general, % perfil, fecha de entrega, nota de presentación del admin.
- Clic → perfil completo del candidato (ya existe: `/applicant-profile/{candidateId}`).
- Acciones por card según estado:
  - `Delivered`/`ViewedByCompany`: botones "Me interesa" y "No encaja".
  - `Interested`: botones "Contratar" / "Descartar".
  - `Hired` / `RejectedByCompany`: estado final, sin acciones (mostrar feedback si existe).
- Al abrir el perfil de un entregado con estado `Delivered`, marcar `ViewedByCompany` (llamar respond con status 1, o hacerlo automático en el endpoint de perfil — más simple: al cargar la lista, si `ViewedAt == null` no tocar; agregar llamada explícita al abrir perfil).

**Cliente HTTP — `ApiAuthService.cs` (OpenToWork.WEB):**

```csharp
public async Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid? vacancyId = null)
{
    await SetAuthHeaderAsync();
    var url = vacancyId.HasValue ? $"api/deliveries/my?vacancyId={vacancyId}" : "api/deliveries/my";
    var response = await _httpClient.GetAsync(url);
    return await response.Content.ReadFromJsonAsync<List<DeliveryDto>>() ?? new();
}

public async Task<VacancyApplicantSummaryDto?> GetVacancySummaryAsync(Guid vacancyId)
{
    await SetAuthHeaderAsync();
    return await _httpClient.GetFromJsonAsync<VacancyApplicantSummaryDto>($"api/deliveries/vacancy-summary/{vacancyId}");
}

public async Task<bool> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback)
{
    await SetAuthHeaderAsync();
    var response = await _httpClient.PutAsJsonAsync($"api/deliveries/{deliveryId}/respond",
        new RespondDeliveryDto { Status = status, Feedback = feedback });
    return response.IsSuccessStatusCode;
}
```

---

## Paso 6: i18n

Agregar claves en `src/OpenToWork.WEB/wwwroot/config/language/{es,en}/dashboard.json` (o `common.json` según dónde vivan las pantallas):

| Clave | ES | EN |
|-------|----|----|
| `vacancy.applicantsTotal` | `{0} postulantes` | `{0} applicants` |
| `vacancy.inVerification` | `{0} en verificación` | `{0} under verification` |
| `delivery.personalVerified` | Personal verificado | Verified personnel |
| `delivery.empty` | Aún no hay personal verificado entregado para esta vacante | No verified personnel delivered for this vacancy yet |
| `delivery.interested` | Me interesa | Interested |
| `delivery.hired` | Contratado | Hired |
| `delivery.rejected` | No encaja | Not a fit |
| `delivery.deliveredBy` | Entregado por Trato Directo el {0} | Delivered by Trato Directo on {0} |

Y en `src/OpenToWork.AdminWEB/wwwroot/config/language/{es,en}/admin.json`:

| Clave | ES | EN |
|-------|----|----|
| `admin.recruitment.deliver` | Entregar a empresa | Deliver to company |
| `admin.recruitment.deliverTitle` | Entregar candidato verificado | Deliver verified candidate |
| `admin.recruitment.adminNote` | Nota de presentación | Presentation note |
| `admin.recruitment.delivered` | Entregado | Delivered |
| `admin.recruitment.requiresVerifiedTD` | El candidato debe estar Verificado TD antes de ser entregado | Candidate must be Verified TD before delivery |

---

## Paso 7: Cómo verificar que cumple (checklist de validación RH)

Ejecutar AdminAPI + AdminWEB + API + WEB y validar en navegador con datos reales:

### A) Confidencialidad (lo crítico)
- [ ] Login como empresa → vacantes → **NO se ven nombres ni perfiles de postulantes** en ninguna vista. Solo conteos.
- [ ] Inspeccionar la respuesta de red del endpoint de aplicaciones: **no contiene** `CandidateName`, ni emails, ni datos de candidatos.
- [ ] Intentar `GET api/applications/vacancy/{id}` como empresa → devuelve solo conteos (o 403 si se deprecó).

### B) Entrega desde admin
- [ ] Candidato en stage 4 SIN Verificado TD → botón "Entregar" deshabilitado o error claro.
- [ ] Candidato en stage 4 CON Verificado TD → entrega exitosa, aparece en la sección "Entregas".
- [ ] Intentar entregar dos veces el mismo candidato a la misma vacante → rechazado.

### C) Vista empresa
- [ ] Tras la entrega, la empresa ve al candidato en "Personal Verificado" con badge Verificado TD.
- [ ] La empresa marca "Me interesa" → estado cambia; el admin lo ve en la sección Entregas del pipeline.
- [ ] La empresa marca "No encaja" con feedback → visible para el admin.
- [ ] La empresa NO puede cambiar el estado de postulaciones (endpoint `PUT applications/{id}/status` eliminado o bloqueado).

### D) Conteos
- [ ] Vacante con 5 postulantes, 2 en pipeline, 1 entregado → muestra "5 postulantes, 2 en verificación" (o 1 en verificación según definición final: postulados sin delivery).
- [ ] Los conteos coinciden con lo que ve el admin en el pipeline.

### E) Regresión
- [ ] El postulante sigue pudiendo aplicar y ver sus propias postulaciones (`GET api/applications/my` intacto).
- [ ] El admin sigue viendo el pipeline completo sin cambios.
- [ ] Build sin errores: `dotnet build OpenToWork.slnx`.

---

## Orden de trabajo sugerido

| Orden | Paso | Esfuerzo |
|-------|------|----------|
| 1º | Paso 1: Entidad + enum + migración | Bajo |
| 2º | Paso 2: DeliveryService + DI | Medio |
| 3º | Paso 3: Endpoints (Admin y API) | Medio |
| 4º | Paso 4: UI Admin "Entregar a empresa" | Medio |
| 5º | Paso 5: UI Empresa "Personal Verificado" + summary | Medio |
| 6º | Paso 6: i18n | Bajo |
| 7º | Paso 7: Checklist de validación | — |

## Notas

- **Diseño:** los cambios visuales requieren tu autorización (regla del proyecto); esta guía define comportamiento, no estética. Usar tokens existentes.
- **Seguridad:** la regla de oro es que **ningún endpoint del rol empresa devuelva datos de postulantes sin delivery activo**. Validar siempre `Company.SCUserId == userId` del token.
- **Migración:** una sola migración para la entidad nueva; no mezclar con otros cambios.
- **Commits:** uno por paso, mensajes descriptivos.
