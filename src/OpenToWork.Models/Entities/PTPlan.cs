using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Models.Entities;

public class PTPlan : BaseEntity
{
    public PlanAudience Audience { get; set; } = PlanAudience.Company;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "EUR";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    /// <summary>Beneficios del plan, uno por línea.</summary>
    public string? Features { get; set; }
}
