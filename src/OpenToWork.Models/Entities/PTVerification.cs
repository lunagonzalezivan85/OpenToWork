using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Una fila por tipo de verificacion (Type) por candidato. Se crea bajo demanda, cuando esa
/// verificacion corre por primera vez - no se pre-crean las 6 filas al registrar el candidato
/// (ver fase-3-sub1.md). Ver plan obligatorio de Fase 3 en README, sub-fase 3.1/3.2.
/// </summary>
public class PTVerification : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    /// <summary>OpenToWork.Shared.Enums.VerificationType: Identity=0, LinkedIn=1, Portfolio=2, CvCoherence=3, Education=4, Reference=5</summary>
    public int Type { get; set; }

    /// <summary>OpenToWork.Shared.Enums.VerificationCheckStatus: Pending=0, InProgress=1, Verified=2, Failed=3</summary>
    public int Status { get; set; } = 0;

    public DateTime? VerifiedAt { get; set; }

    /// <summary>JSON con el detalle del resultado de la verificacion (issues encontrados, etc).</summary>
    public string? Result { get; set; }

    public int Score { get; set; } = 0;
}
