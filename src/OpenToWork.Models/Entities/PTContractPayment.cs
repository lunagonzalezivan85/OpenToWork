using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Tramo de pago del anexo de contrato (Apertura 30% / Validacion 50% / Consolidacion 20%).
/// Se crean los 3 automaticamente al aceptar el contrato, con el monto congelado segun
/// el FeeAmount y el % vigente en ese momento. Sin pasarela de pago integrada: un admin
/// los marca pagado/pendiente a mano.
/// </summary>
public class PTContractPayment : BaseEntity
{
    [Required]
    public Guid PT_VacancyContractId { get; set; }

    [ForeignKey("PT_VacancyContractId")]
    public virtual PTVacancyContract Contract { get; set; } = null!;

    /// <summary>PaymentTrancheType: Apertura / Validacion / Consolidacion.</summary>
    public int TrancheType { get; set; }

    /// <summary>% de este tramo al momento de crearlo (copia de PTVacancyContract, para que un
    /// cambio posterior en el contrato no altere tramos ya generados).</summary>
    public decimal Percentage { get; set; }

    /// <summary>Monto = FeeAmount * Percentage / 100, congelado al crear el tramo.</summary>
    public decimal Amount { get; set; }

    /// <summary>PaymentTrancheStatus: Pendiente / Pagado.</summary>
    public int Status { get; set; } = 0;

    public DateTime? PaidAt { get; set; }

    public Guid? PaidByUserId { get; set; }

    [ForeignKey("PaidByUserId")]
    public virtual SCUser? PaidByUser { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
