using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTTechnicalEvaluation : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment CandidateRecruitment { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string EvaluationName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 100)]
    public decimal Score { get; set; }

    [MaxLength(500)]
    public string? EvidenceUrl { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime? EvaluatedAt { get; set; }

    [Required]
    public Guid EvaluatedByUserId { get; set; }

    [ForeignKey("EvaluatedByUserId")]
    public virtual SCUser EvaluatedByUser { get; set; } = null!;
}
