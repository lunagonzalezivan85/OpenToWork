using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCompanyPipeline : BaseEntity
{
    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    public int CurrentStage { get; set; } = 0;

    public Guid? AssignedToUserId { get; set; }

    [ForeignKey("AssignedToUserId")]
    public virtual SCUser? AssignedToUser { get; set; }

    public Guid? AssignedByUserId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? StageEnteredAt { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsDismissed { get; set; } = false;

    [MaxLength(500)]
    public string? DismissalReason { get; set; }

    public DateTime? DismissedAt { get; set; }

    public Guid? DismissedByUserId { get; set; }

    [ForeignKey("DismissedByUserId")]
    public virtual SCUser? DismissedByUser { get; set; }

    public virtual ICollection<PTCompanyStageLog> StageLogs { get; set; } = new List<PTCompanyStageLog>();
}
