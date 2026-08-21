using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTInvestigationChecklist : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    public int Step { get; set; }

    [MaxLength(200)]
    public string? Label { get; set; }

    public bool IsCustom { get; set; } = false;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public Guid? CompletedByUserId { get; set; }

    [ForeignKey("CompletedByUserId")]
    public virtual SCUser? CompletedByUser { get; set; }

    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? EvidenceUrl { get; set; }
}
