using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTRecruitmentDismissal : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    public int Reason { get; set; }

    public string? Notes { get; set; }

    public Guid DismissedByUserId { get; set; }

    [ForeignKey("DismissedByUserId")]
    public virtual SCUser DismissedByUser { get; set; } = null!;
}
