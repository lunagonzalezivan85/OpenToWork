using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Postulacion incluida en una ronda de negociacion presentada a la empresa.
/// </summary>
public class PTNegotiationCandidate : BaseEntity
{
    [Required]
    public Guid PT_NegotiationId { get; set; }

    [ForeignKey("PT_NegotiationId")]
    public virtual PTNegotiation Negotiation { get; set; } = null!;

    [Required]
    public Guid PT_ApplicationId { get; set; }

    [ForeignKey("PT_ApplicationId")]
    public virtual PTApplication Application { get; set; } = null!;
}
