using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTRecruitmentStageLog : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    public int FromStage { get; set; }

    public int ToStage { get; set; }

    public Guid ChangedByUserId { get; set; }

    [ForeignKey("ChangedByUserId")]
    public virtual SCUser ChangedByUser { get; set; } = null!;

    public string? Notes { get; set; }
}
