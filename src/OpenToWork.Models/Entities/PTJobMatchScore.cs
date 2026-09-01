using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Job Match Score: por par candidato-vacante, distinto del Candidate Score intrinseco
/// (PTCandidateScore). La empresa puede ajustar los pesos por vacante via WeightsConfig.
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.1/3.4.
/// </summary>
public class PTJobMatchScore : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    public int MatchPercentage { get; set; } = 0;

    public int SkillsMatch { get; set; } = 0;

    public int ExperienceMatch { get; set; } = 0;

    public int EducationMatch { get; set; } = 0;

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON con los pesos usados en este calculo, ej: {"skills":40,"experience":30,"education":20,"location":10}
    /// (porcentajes que suman 100). Ver fase-3-sub1.md.
    /// </summary>
    public string? WeightsConfig { get; set; }
}
