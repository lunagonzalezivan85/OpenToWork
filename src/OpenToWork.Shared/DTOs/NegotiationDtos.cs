namespace OpenToWork.Shared.DTOs;

public class NegotiationCandidateDto
{
    public Guid ApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public int ApplicationStatus { get; set; }
}

public class NegotiationDto
{
    public Guid Id { get; set; }
    public Guid VacancyId { get; set; }
    public string? VacancyTitle { get; set; }
    public int Status { get; set; }
    public Guid? AssignedStaffId { get; set; }
    public DateTime? PresentedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? WinningApplicationId { get; set; }
    public string? Notes { get; set; }
    public List<NegotiationCandidateDto> Candidates { get; set; } = new();

    public DateTime? IncorporationDate { get; set; }

    /// <summary>IncorporationDate + WarrantyDays del contrato de la vacante. Null si falta la
    /// fecha de incorporacion o el contrato no define garantia.</summary>
    public DateTime? WarrantyEndsAt { get; set; }

    /// <summary>WarrantyStatus (Activa/PorVencer/Vencida), o null si WarrantyEndsAt es null.</summary>
    public int? WarrantyStatus { get; set; }

    /// <summary>true si ya existe una reposicion de garantia EnCurso sobre esta negociacion
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

public class CreateNegotiationDto
{
    public Guid VacancyId { get; set; }

    /// <summary>
    /// Candidatos del shortlist (compatibilidad calculada, Fase 3) seleccionados para presentar.
    /// No requieren haber aplicado antes: si no existe una PT_Application para el par
    /// candidato-vacante, NegotiationService la crea con ApplicationSource=AdminCurated.
    /// </summary>
    public List<Guid> CandidateIds { get; set; } = new();
}

public class UpdateNegotiationStatusDto
{
    public int Status { get; set; }
}

public class CloseNegotiationDto
{
    public Guid WinningApplicationId { get; set; }
}

public class SetIncorporationDateDto
{
    public DateTime IncorporationDate { get; set; }
}

public class CloseProcessDto
{
    public string? Notes { get; set; }
}

public class RecordFeedbackDto
{
    public int Rating { get; set; }
    public string? Comments { get; set; }
}
