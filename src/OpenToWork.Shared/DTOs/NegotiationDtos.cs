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
