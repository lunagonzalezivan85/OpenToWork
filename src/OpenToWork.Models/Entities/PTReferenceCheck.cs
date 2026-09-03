using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTReferenceCheck : BaseEntity
{
    [Required]
    public Guid PT_InvestigationChecklistId { get; set; }

    [ForeignKey("PT_InvestigationChecklistId")]
    public virtual PTInvestigationChecklist Checklist { get; set; } = null!;

    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    public int Status { get; set; } = 0; // 0=Pending, 1=Called, 2=Validated, 3=NoResponse

    public DateTime? CalledAt { get; set; }

    public string? Notes { get; set; }
}
