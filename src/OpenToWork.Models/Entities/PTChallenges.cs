using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

// Retos de hosteleria ("Demuestra tus habilidades"). Ver docs/dsiezar/retos-hosteleria.md.
// El contenido de cada reto es una definicion JSON (OpenToWork.Shared.Challenges.ChallengeDefinition):
// borrador editable en PTChallenge.DraftJson y copia inmutable en PTChallengeVersion.ContentJson.

/// <summary>Competencia evaluable (priorizacion, calculo aplicado...). No se borra: se desactiva,
/// para conservar las referencias de versiones e intentos anteriores.</summary>
public class PTCompetency : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>Clave estable del contenido inicial (carga idempotente). Null si se creo desde el admin.</summary>
    [MaxLength(80)]
    public string? Slug { get; set; }
}

/// <summary>Competencias asociadas a un cargo del catalogo (PTJobType).</summary>
public class PTJobTypeCompetency : BaseEntity
{
    public Guid PT_JobTypeId { get; set; }
    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType JobType { get; set; } = null!;

    public Guid PT_CompetencyId { get; set; }
    [ForeignKey("PT_CompetencyId")]
    public virtual PTCompetency Competency { get; set; } = null!;
}

public class PTChallenge : BaseEntity
{
    /// <summary>Clave estable (unica). El contenido inicial se identifica por ella para no duplicarlo.</summary>
    [Required]
    [MaxLength(80)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Titulo del borrador (copia para listados; la verdad esta en DraftJson).</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>OpenToWork.Shared.Challenges.ChallengeStatus: Draft=0, Published=1, Archived=2.</summary>
    public int Status { get; set; }

    /// <summary>Definicion editable (borrador). Los candidatos nunca la ven: solo versiones publicadas.</summary>
    [Column(TypeName = "longtext")]
    public string DraftJson { get; set; } = "{}";

    /// <summary>El borrador tiene cambios que aun no se han publicado como nueva version.</summary>
    public bool HasDraftChanges { get; set; } = true;

    public Guid? LatestVersionId { get; set; }
    public int? LatestVersionNumber { get; set; }

    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedBy { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }

    public virtual ICollection<PTChallengeJobType> JobTypes { get; set; } = new List<PTChallengeJobType>();
    public virtual ICollection<PTChallengeVersion> Versions { get; set; } = new List<PTChallengeVersion>();
}

/// <summary>Un mismo reto asignado a varios cargos sin duplicar su contenido.</summary>
public class PTChallengeJobType : BaseEntity
{
    public Guid PT_ChallengeId { get; set; }
    [ForeignKey("PT_ChallengeId")]
    public virtual PTChallenge Challenge { get; set; } = null!;

    public Guid PT_JobTypeId { get; set; }
    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType JobType { get; set; } = null!;

    public int SortOrder { get; set; }
}

/// <summary>Version publicada e inmutable: actividades, recursos, competencias, reglas y rubricas.</summary>
public class PTChallengeVersion : BaseEntity
{
    public Guid PT_ChallengeId { get; set; }
    [ForeignKey("PT_ChallengeId")]
    public virtual PTChallenge Challenge { get; set; } = null!;

    public int VersionNumber { get; set; }

    [Column(TypeName = "longtext")]
    public string ContentJson { get; set; } = "{}";

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public Guid? PublishedBy { get; set; }
}

public class PTChallengeAttempt : BaseEntity
{
    public Guid PT_CandidateId { get; set; }
    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    public Guid PT_ChallengeId { get; set; }
    [ForeignKey("PT_ChallengeId")]
    public virtual PTChallenge Challenge { get; set; } = null!;

    /// <summary>Version con la que empezo: no cambia aunque el reto se edite o archive despues.</summary>
    public Guid PT_ChallengeVersionId { get; set; }
    [ForeignKey("PT_ChallengeVersionId")]
    public virtual PTChallengeVersion Version { get; set; } = null!;

    /// <summary>AttemptMode: Practice=0, Evaluation=1.</summary>
    public int Mode { get; set; }
    /// <summary>AttemptStatus: InProgress=0, Submitted=1.</summary>
    public int Status { get; set; }
    /// <summary>ReviewStatus: NotRequired=0, Pending=1, InProgress=2, Completed=3.</summary>
    public int ReviewStatus { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    /// <summary>Duracion registrada (no cuenta en la nota).</summary>
    public int DurationSeconds { get; set; }
    public DateTime? ConditionsAcceptedAt { get; set; }

    /// <summary>Resultado calculado (ChallengeResult en JSON). Se recalcula al entregar y al revisar.</summary>
    [Column(TypeName = "longtext")]
    public string? ResultJson { get; set; }

    /// <summary>Token de concurrencia: dos "Entregar" simultaneos no entregan dos veces.</summary>
    [ConcurrencyCheck]
    public int RowVersion { get; set; }

    public virtual ICollection<PTChallengeAnswer> Answers { get; set; } = new List<PTChallengeAnswer>();
}

/// <summary>Una fila por actividad e intento (indice unico): reenviar o recargar no duplica respuestas.</summary>
public class PTChallengeAnswer : BaseEntity
{
    public Guid PT_ChallengeAttemptId { get; set; }
    [ForeignKey("PT_ChallengeAttemptId")]
    public virtual PTChallengeAttempt Attempt { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string ActivityKey { get; set; } = string.Empty;

    [Column(TypeName = "longtext")]
    public string ResponseJson { get; set; } = "{}";

    /// <summary>Parte automatica 0-1 (null si la actividad es solo de revision humana).</summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? AutoScore { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Veces que se ha guardado (en practica se puede reintentar).</summary>
    public int SaveCount { get; set; } = 1;
}

/// <summary>Revision humana de un intento (una por intento).</summary>
public class PTChallengeReview : BaseEntity
{
    public Guid PT_ChallengeAttemptId { get; set; }
    [ForeignKey("PT_ChallengeAttemptId")]
    public virtual PTChallengeAttempt Attempt { get; set; } = null!;

    public Guid ReviewerUserId { get; set; }

    /// <summary>List&lt;ReviewScoreDto&gt; en JSON: puntuacion 0-4 y observacion por criterio.</summary>
    [Column(TypeName = "longtext")]
    public string ScoresJson { get; set; } = "[]";

    [MaxLength(2000)]
    public string? GeneralComment { get; set; }

    public DateTime? CompletedAt { get; set; }
}
