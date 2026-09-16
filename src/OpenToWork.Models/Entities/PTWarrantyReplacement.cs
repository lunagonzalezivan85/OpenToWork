using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Activacion de garantia de reposicion sobre una colocacion (negociacion o entrega) que fallo
/// durante el periodo de garantia. Registra la causa raiz, si cuenta como 1a (gratis) o 2a
/// (50% del FeeAmount) reposicion, y el vinculo con la nueva colocacion que la resuelve.
/// </summary>
public class PTWarrantyReplacement : BaseEntity
{
    [Required]
    public Guid PT_VacancyContractId { get; set; }

    [ForeignKey("PT_VacancyContractId")]
    public virtual PTVacancyContract Contract { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    /// <summary>Colocacion original que fallo. Exactamente uno de los dos segun el flujo que
    /// activo la reposicion (Negociaciones o Embudo Ciego).</summary>
    public Guid? OriginalNegotiationId { get; set; }

    [ForeignKey("OriginalNegotiationId")]
    public virtual PTNegotiation? OriginalNegotiation { get; set; }

    public Guid? OriginalDeliveryId { get; set; }

    [ForeignKey("OriginalDeliveryId")]
    public virtual PTCandidateDelivery? OriginalDelivery { get; set; }

    /// <summary>WarrantyReplacementReason.</summary>
    public int Reason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Snapshot de WarrantyReplacementReasonExtensions.IsExclusion(Reason) al crear.</summary>
    public bool IsExclusion { get; set; }

    /// <summary>1 o 2 (0 si IsExclusion - no cuenta como reposicion cubierta).</summary>
    public int ReplacementNumber { get; set; }

    /// <summary>WarrantyReplacementStatus.</summary>
    public int Status { get; set; }

    public Guid RequestedByUserId { get; set; }

    [ForeignKey("RequestedByUserId")]
    public virtual SCUser RequestedByUser { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    /// <summary>Nueva colocacion que resuelve la reposicion, una vez vinculada.</summary>
    public Guid? ReplacementNegotiationId { get; set; }

    [ForeignKey("ReplacementNegotiationId")]
    public virtual PTNegotiation? ReplacementNegotiation { get; set; }

    public Guid? ReplacementDeliveryId { get; set; }

    [ForeignKey("ReplacementDeliveryId")]
    public virtual PTCandidateDelivery? ReplacementDelivery { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>Tramo de cargo del 50% (PTContractPayment, TrancheType=ReposicionSegunda),
    /// solo cuando ReplacementNumber == 2.</summary>
    public Guid? ChargeTrancheId { get; set; }

    [ForeignKey("ChargeTrancheId")]
    public virtual PTContractPayment? ChargeTranche { get; set; }
}
