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
    public string CompanyName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public string? CompanyFeedback { get; set; }
    public int? RejectionReason { get; set; }
    public DateTime DeliveredAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int OverallScore { get; set; }
    public int ProfileCompletionPercentage { get; set; }
    public bool IsVerifiedTD { get; set; }

    /// <summary>Paso 16: fecha en que la empresa contrata formalmente al candidato (firma su
    /// contrato laboral, gestionado por la empresa, no por TD).</summary>
    public DateTime? HiringDate { get; set; }

    public DateTime? IncorporationDate { get; set; }

    /// <summary>IncorporationDate + WarrantyDays del contrato de la vacante. Null si falta la
    /// fecha de incorporacion o el contrato no define garantia.</summary>
    public DateTime? WarrantyEndsAt { get; set; }

    /// <summary>WarrantyStatus (Activa/PorVencer/Vencida), o null si WarrantyEndsAt es null.</summary>
    public int? WarrantyStatus { get; set; }

    /// <summary>true si ya existe una reposicion de garantia EnCurso sobre esta entrega
    /// (evita activar una segunda mientras la primera sigue abierta).</summary>
    public bool HasActiveWarrantyReplacement { get; set; }

    /// <summary>Paso 21: fecha en que se cerro administrativamente el proceso (garantia vencida
    /// o sin garantia definida, sin reposiciones sin resolver). Null si sigue abierto.</summary>
    public DateTime? ProcessClosedAt { get; set; }
    public string? ProcessClosedByName { get; set; }
    public string? ProcessClosureNotes { get; set; }

    /// <summary>true si hoy se cumplen las condiciones para cerrar el proceso (calculado en el
    /// servidor para no duplicar la regla en el cliente).</summary>
    public bool CanCloseProcess { get; set; }

    /// <summary>Paso 22: feedback de mejora continua, solo se puede registrar una vez cerrado
    /// el proceso.</summary>
    public int? FeedbackRating { get; set; }
    public string? FeedbackComments { get; set; }
    public DateTime? FeedbackRecordedAt { get; set; }
    public bool CanRecordFeedback { get; set; }

    /// <summary>"Liberar candidato": el candidato dejo este puesto fuera de garantia.</summary>
    public DateTime? PlacementEndedAt { get; set; }
    public int? PlacementEndReason { get; set; }
    public string? PlacementEndNotes { get; set; }
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
    public int Status { get; set; }
    public string? Feedback { get; set; }
    /// <summary>DeliveryRejectionReason, obligatorio si Status = RejectedByCompany.</summary>
    public int? RejectionReason { get; set; }
}

/// <summary>Resumen de entregas de un candidato para las listas del admin (pipeline, consola,
/// perfil). Calculado por CandidatePlacementHelper - ver ahi la regla de "Colocado".</summary>
public class CandidatePlacementSummaryDto
{
    public bool IsPlaced { get; set; }
    public string? PlacedCompanyName { get; set; }
    public string? PlacedVacancyTitle { get; set; }
    public int DeliveriesCount { get; set; }
    public int RejectedCount { get; set; }
    public int? LastDeliveryStatus { get; set; }
    public string? LastDeliveryCompanyName { get; set; }
    public DateTime? LastDeliveredAt { get; set; }
    /// <summary>La ultima entrega fue Contratado pero el candidato ya dejo el puesto (reposicion o liberado).</summary>
    public bool LastDeliveryLeft { get; set; }
    /// <summary>Candidato "quemado": BurnedRejectionThreshold o mas rechazos y nunca contratado.</summary>
    public bool IsBurned { get; set; }
}

public class ReleaseCandidateDto
{
    /// <summary>PlacementEndReason.</summary>
    public int Reason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Historial de entregas de un candidato (todas las empresas/vacantes), para detectar
/// candidatos "quemados" (muchos rechazos sin ninguna contratacion).</summary>
public class CandidateDeliveryHistoryDto
{
    public int TotalDeliveries { get; set; }
    public int DistinctCompanies { get; set; }
    public int HiredCount { get; set; }
    public int RejectedCount { get; set; }
    /// <summary>Entregadas sin respuesta final (Entregado/Visto/Interesado).</summary>
    public int PendingCount { get; set; }
    /// <summary>RejectedCount >= CandidatePlacementHelper.BurnedRejectionThreshold y nunca contratado.</summary>
    public bool IsBurned { get; set; }
    public int BurnedThreshold { get; set; }
    /// <summary>Rechazos agrupados por DeliveryRejectionReason (null = registrado antes de existir el motivo).</summary>
    public List<RejectionReasonCountDto> RejectionsByReason { get; set; } = new();
    public List<DeliveryHistoryItemDto> Items { get; set; } = new();
}

public class RejectionReasonCountDto
{
    public int? Reason { get; set; }
    public int Count { get; set; }
}

public class DeliveryHistoryItemDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int Status { get; set; }
    public int? RejectionReason { get; set; }
    public string? CompanyFeedback { get; set; }
    public DateTime? PlacementEndedAt { get; set; }
}
