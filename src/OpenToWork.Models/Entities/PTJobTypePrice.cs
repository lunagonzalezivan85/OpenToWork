using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Registro de precio base para un tipo de puesto (lista de precios B2B). Versionado:
/// nunca se sobreescribe un precio, se cierra el vigente (EffectiveTo) y se crea uno
/// nuevo. Asi un contrato ya firmado conserva la tarifa que tenia al momento de
/// aceptarse, aunque la lista de precios cambie despues (clausula 6.3 del Contrato Marco).
/// </summary>
public class PTJobTypePrice : BaseEntity
{
    [Required]
    public Guid PT_JobTypeId { get; set; }

    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType JobType { get; set; } = null!;

    public decimal BasePrice { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    /// <summary>Null = precio vigente. Se completa automaticamente al crear el siguiente registro.</summary>
    public DateTime? EffectiveTo { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
