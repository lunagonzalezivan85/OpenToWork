using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Candidate Score: intrinseco del candidato, no depende de ninguna vacante en particular
/// (distinto de PTJobMatchScore, que es por par candidato-vacante). Ver plan obligatorio de
/// Fase 3 en README, sub-fase 3.1.
/// </summary>
public class PTCandidateScore : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    public int StabilityIndex { get; set; } = 0;

    public int ReliabilityIndex { get; set; } = 0;

    public int EvidenceIndex { get; set; } = 0;

    public int CompatibilityIndex { get; set; } = 0;

    public int OverallScore { get; set; } = 0;

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Se incrementa en cada recalculo (estrategia incremental, ver fase-3-sub1.md).</summary>
    public int Version { get; set; } = 1;
}
