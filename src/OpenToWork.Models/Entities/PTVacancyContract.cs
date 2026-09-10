using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Anexo de contrato de servicio por vacante (TD -> empresa solicitante). Establece alcance,
/// plazos, garantia y condiciones economicas. 1:1 con la vacante (indice unico). Solo editable
/// en estado Draft; Accepted/Rejected/Cancelled quedan bloqueados. Secciones 5/6/7 del anexo.
/// </summary>
public class PTVacancyContract : BaseEntity
{
    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string ContractNumber { get; set; } = string.Empty;

    public int Status { get; set; } = 0;

    /// <summary>Seccion 5: actuaciones contratadas, JSON array de codigos (publicacion, busqueda, screening, entrevista, evaluacion, referencias, shortlist, briefing, seguimiento).</summary>
    [MaxLength(500)]
    public string? ScopeServices { get; set; }

    /// <summary>N objetivo de candidatos (meta de captacion, heredado de la vacante).</summary>
    public int? TargetCandidates { get; set; }

    /// <summary>Tipo de puesto para plazos de referencia (ContractJobType).</summary>
    public int JobTypeCategory { get; set; } = 0;

    /// <summary>Plazo objetivo de cobertura en dias habiles.</summary>
    public int? TargetCoverageDays { get; set; }

    /// <summary>Seccion 6: periodo de garantia en dias naturales.</summary>
    public int? WarrantyDays { get; set; }

    /// <summary>Seccion 7: tarifa sin IVA.</summary>
    public decimal? FeeAmount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    /// <summary>Aplicacion de tarifa (FeeApplicationType): por posicion / por proceso / precio global.</summary>
    public int FeeApplicationType { get; set; } = 0;

    /// <summary>Distribucion de pago: % apertura (default 30).</summary>
    public decimal PaymentOpeningPct { get; set; } = 30m;

    /// <summary>% validacion (default 50).</summary>
    public decimal PaymentValidationPct { get; set; } = 50m;

    /// <summary>% consolidacion (default 20).</summary>
    public decimal PaymentConsolidationPct { get; set; } = 20m;

    /// <summary>Excepciones pactadas (seccion 7).</summary>
    public string? FeeExceptions { get; set; }

    /// <summary>Fecha en que la empresa acepto el anexo (null = pendiente/rechazado).</summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>Fecha de rechazo por la empresa.</summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>Motivo del rechazo (obligatorio al rechazar).</summary>
    [MaxLength(1000)]
    public string? RejectionReason { get; set; }
}
