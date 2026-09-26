using OpenToWork.Shared.Enums;

namespace OpenToWork.Shared.DTOs;

/// <summary>Vista del propio candidato sobre su proceso con Trato Directo (portal, /my-process).
/// Solo datos que el candidato puede ver: sin notas internas, reclutador asignado ni motivos de descarte.</summary>
public class CandidateProcessDto
{
    /// <summary>False si Trato Directo aun no le abrio un proceso de seleccion.</summary>
    public bool HasRecruitment { get; set; }

    /// <summary>RecruitmentStage (0-5). 5 = Descartado, que el portal muestra como "proceso finalizado".</summary>
    public int CurrentStage { get; set; }

    public DateTime? StageEnteredAt { get; set; }

    public string? VacancyTitle { get; set; }

    public string? VacancyCompanyName { get; set; }

    /// <summary>Primera fecha en que alcanzo cada etapa (clave = RecruitmentStage).</summary>
    public Dictionary<int, DateTime> StageReachedAt { get; set; } = new();

    public List<CandidateProcessDeliveryDto> Deliveries { get; set; } = new();

    public bool PlanFeatureEnabled { get; set; }

    public CandidatePlanTier PlanTier { get; set; }

    public DateTime? PlanExpiresAt { get; set; }

    public bool PlanIsActive { get; set; }
}

public class CandidateProcessDeliveryDto
{
    public Guid VacancyId { get; set; }

    public string VacancyTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public DateTime DeliveredAt { get; set; }

    /// <summary>DeliveryStatus. El motivo de un rechazo no se envia al candidato.</summary>
    public int Status { get; set; }

    public DateTime? IncorporationDate { get; set; }

    /// <summary>Dejo el puesto (PlacementEndedAt).</summary>
    public bool PlacementEnded { get; set; }
}
