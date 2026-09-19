namespace OpenToWork.Shared.DTOs;

public class AdminApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Id de PTCandidate (distinto de UserId que es SCUser.Id).</summary>
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public DateTime? AvailableFromDate { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Verificado: reclutamiento en etapa 4 o checks automáticos completos (misma regla que la consola de candidatos).</summary>
    public bool IsVerifiedTD { get; set; }
}
