using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Solicitud de verificacion de identidad y habilitacion laboral (wizard /verification-request).
/// Una fila por solicitud enviada por el candidato. Las ramas del wizard se guardan en
/// AdminSituation: 0=Espanola, 1=Comunitario UE/EEE/Suiza, 2=Extracomunitario con TIE,
/// 3=Sin permiso de trabajo (solo datos de contacto - minimizacion RGPD).
/// </summary>
public class PTVerificationRequest : BaseEntity
{
    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    /// <summary>Numero de referencia publico, formato TD-XXXXXXXX.</summary>
    [Required]
    [MaxLength(20)]
    public string ReferenceNumber { get; set; } = "";

    /// <summary>Respuesta al filtro principal: nacionalidad espanola.</summary>
    public bool HasSpanishNationality { get; set; }

    /// <summary>0=Espanola, 1=Comunitario (UE/EEE/Suiza), 2=Extracomunitario con TIE, 3=Sin permiso.</summary>
    public int AdminSituation { get; set; }

    [MaxLength(200)]
    public string? FullName { get; set; }

    /// <summary>DNI (rama 1) o NIE/TIE (ramas 2 y 3).</summary>
    [MaxLength(20)]
    public string? DocumentNumber { get; set; }

    public DateTime? DocumentExpiry { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }

    /// <summary>Solo rama 3: 0=Cuenta Ajena, 1=Cuenta Propia, 2=Larga Duracion, 3=Comunitario.</summary>
    public int? PermitType { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>0=Manana (09:00-14:00), 1=Tarde (15:00-19:00).</summary>
    public int? ContactTimePreference { get; set; }

    /// <summary>Paths relativos de los documentos subidos (uploads/verification/...).</summary>
    [MaxLength(500)]
    public string? DocFrontPath { get; set; }

    [MaxLength(500)]
    public string? DocBackPath { get; set; }

    [MaxLength(500)]
    public string? DocExtra1Path { get; set; }

    [MaxLength(500)]
    public string? DocExtra2Path { get; set; }

    /// <summary>0=Pending, 1=InReview, 2=Approved, 3=Rejected.</summary>
    public int Status { get; set; } = 0;

    /// <summary>Traza del consentimiento (RGPD): fecha, IP y version de terminos aceptados.</summary>
    public DateTime ConsentAcceptedAt { get; set; }

    [MaxLength(50)]
    public string? ConsentIp { get; set; }

    [MaxLength(10)]
    public string TermsVersion { get; set; } = "1.0";
}
