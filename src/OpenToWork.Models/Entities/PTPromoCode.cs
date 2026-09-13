using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Codigo promocional para descuentos en la tarifa de contratacion por vacante.
/// Se puede restringir a un nivel de puesto y/o a un tipo de puesto especifico
/// (ej: "ALEJO26" = 10% en vacantes de nivel Operativo). Sin restriccion = aplica
/// a cualquier vacante.
/// </summary>
public class PTPromoCode : BaseEntity
{
    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>PromoDiscountType: Percentage=0, FixedAmount=1.</summary>
    public int DiscountType { get; set; }

    /// <summary>Si DiscountType=Percentage, valor 0-100. Si FixedAmount, monto en la moneda del precio.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>Restringe el codigo a un nivel de puesto. Null = aplica a todos.</summary>
    public Guid? PT_JobLevelId { get; set; }

    [ForeignKey("PT_JobLevelId")]
    public virtual PTJobLevel? JobLevel { get; set; }

    /// <summary>Restringe el codigo a un tipo de puesto especifico. Null = aplica a todo el nivel (o a todos si tampoco hay nivel).</summary>
    public Guid? PT_JobTypeId { get; set; }

    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType? JobType { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    public DateTime? ValidUntil { get; set; }

    /// <summary>Null = sin limite de usos.</summary>
    public int? MaxUses { get; set; }

    public int UsesCount { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PTPromoCodeRedemption> Redemptions { get; set; } = new List<PTPromoCodeRedemption>();
}
