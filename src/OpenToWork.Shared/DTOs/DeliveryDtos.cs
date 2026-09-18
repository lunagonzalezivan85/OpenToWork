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
}
