using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCompanyStageLog : BaseEntity
{
    [Required]
    public Guid PT_CompanyPipelineId { get; set; }

    [ForeignKey("PT_CompanyPipelineId")]
    public virtual PTCompanyPipeline Pipeline { get; set; } = null!;

    public int FromStage { get; set; }

    public int ToStage { get; set; }

    public Guid ChangedByUserId { get; set; }

    [ForeignKey("ChangedByUserId")]
    public virtual SCUser ChangedByUser { get; set; } = null!;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
