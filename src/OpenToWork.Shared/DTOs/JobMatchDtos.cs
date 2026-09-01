namespace OpenToWork.Shared.DTOs;

public class JobMatchDto
{
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateTitle { get; set; }
    public Guid VacancyId { get; set; }
    public int MatchPercentage { get; set; }
    public int SkillsMatch { get; set; }
    public int ExperienceMatch { get; set; }
    /// <summary>Siempre 0 por ahora - ver fase-3-sub4.md pregunta 5.</summary>
    public int EducationMatch { get; set; }
    public int LocationMatch { get; set; }
    public DateTime CalculatedAt { get; set; }
}
