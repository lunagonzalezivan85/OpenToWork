namespace OpenToWork.Shared.DTOs;

/// <summary>Scorecard configurable de la empresa (fase-3-sub8.md pregunta 4) - los 3 valores deben venir juntos, o ninguno (null limpia la config custom).</summary>
public class UpdateScorecardDto
{
    public double? Skills { get; set; }
    public double? Experience { get; set; }
    public double? Location { get; set; }
}

public class ScorecardDto
{
    public string? WeightsConfig { get; set; }
}

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
