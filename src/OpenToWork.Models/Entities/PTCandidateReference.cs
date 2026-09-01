using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Referencia laboral que el candidato aporta desde su perfil. Distinta de PTReferenceCheck
/// (Iluna, Pipeline de Reclutamiento), que es la verificacion manual de una referencia hecha
/// por un reclutador durante la investigacion. Ver plan obligatorio de Fase 3, sub-fase 3.1/3.5.
/// </summary>
public class PTCandidateReference : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    /// <summary>OpenToWork.Shared.Enums.ReferenceRelationship: Manager=0, Peer=1, Subordinate=2</summary>
    public int Relationship { get; set; } = 0;

    /// <summary>OpenToWork.Shared.Enums.ReferenceStatus: Pending=0, Sent=1, Responded=2, Verified=3, Failed=4</summary>
    public int Status { get; set; } = 0;

    public int? Rating { get; set; }

    public string? Feedback { get; set; }
}
