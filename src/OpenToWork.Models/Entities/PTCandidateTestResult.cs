using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Resultado de un candidato en un PTSkillTest. Ver plan obligatorio de Fase 3 en README,
/// sub-fase 3.1/3.6.
/// </summary>
public class PTCandidateTestResult : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    public Guid PT_SkillTestId { get; set; }

    [ForeignKey("PT_SkillTestId")]
    public virtual PTSkillTest SkillTest { get; set; } = null!;

    public int Score { get; set; } = 0;

    public int TimeTaken { get; set; } = 0;

    public DateTime? CompletedAt { get; set; }

    /// <summary>Contador de banderas anti-copia detectadas (cambio de pestana, etc).</summary>
    public int AntiCheatFlags { get; set; } = 0;
}
