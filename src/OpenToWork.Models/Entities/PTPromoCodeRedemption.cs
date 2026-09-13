using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Auditoria de uso de un codigo promocional: en que linea de contrato (vacante) se
/// aplico, cuanto descuento, cuando. Se conserva aunque el codigo o el precio cambien
/// despues, para no perder trazabilidad de lo ya facturado.
/// </summary>
public class PTPromoCodeRedemption : BaseEntity
{
    public Guid PT_PromoCodeId { get; set; }

    [ForeignKey("PT_PromoCodeId")]
    public virtual PTPromoCode PromoCode { get; set; } = null!;

    public Guid PT_ContractVacancyId { get; set; }

    [ForeignKey("PT_ContractVacancyId")]
    public virtual PTContractVacancy ContractVacancy { get; set; } = null!;

    public decimal DiscountAmountApplied { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
