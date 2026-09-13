using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Tabla de enlace entre contrato y vacantes (1:N). Un contrato agrupa N vacantes
/// de la misma empresa bajo un mismo anexo de servicio. Cada linea lleva su propio
/// precio (snapshot al momento de generar el contrato) porque cada vacante puede ser
/// un tipo de puesto distinto con tarifa distinta.
/// </summary>
public class PTContractVacancy : BaseEntity
{
    [Required]
    public Guid PT_ContractId { get; set; }

    [ForeignKey("PT_ContractId")]
    public virtual PTVacancyContract Contract { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    /// <summary>Snapshot del tipo de puesto de la vacante al momento de generar el contrato.</summary>
    public Guid? PT_JobTypeId { get; set; }

    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType? JobType { get; set; }

    /// <summary>Precio base usado (de la lista de precios, o el manual si IsManualOverride).</summary>
    public decimal? BasePrice { get; set; }

    public Guid? PT_PromoCodeId { get; set; }

    [ForeignKey("PT_PromoCodeId")]
    public virtual PTPromoCode? PromoCode { get; set; }

    /// <summary>Snapshot del texto del codigo: sobrevive aunque el codigo se borre despues.</summary>
    [MaxLength(30)]
    public string? PromoCodeText { get; set; }

    public decimal DiscountAmount { get; set; }

    /// <summary>BasePrice - DiscountAmount. Es lo que realmente se factura por esta vacante.</summary>
    public decimal? FinalPrice { get; set; }

    /// <summary>true = el admin escribio el precio a mano en vez de tomar la lista de precios.</summary>
    public bool IsManualOverride { get; set; }

    [MaxLength(500)]
    public string? OverrideReason { get; set; }
}
