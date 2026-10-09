using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Solicitud de una empresa verificada para que Trato Directo le presente a un candidato
/// que encontro en la busqueda anonima (Fase 2 RGPD / reapertura de H-39). Es una senal
/// para el staff - NO da acceso al perfil: eso solo lo hace una PTCandidateDelivery creada
/// por un admin tras revisar al candidato (CanViewCandidateAsync).
/// </summary>
public class PTCandidateRequest : BaseEntity
{
    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    /// <summary>Usuario de la empresa que hizo la solicitud.</summary>
    [Required]
    public Guid RequestedByUserId { get; set; }

    [ForeignKey("RequestedByUserId")]
    public virtual SCUser RequestedByUser { get; set; } = null!;

    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    /// <summary>Vacante de la empresa para la que lo quieren (opcional pero recomendable).</summary>
    public Guid? PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy? Vacancy { get; set; }

    /// <summary>CandidateRequestStatus: Pending=0, Handled=1 (el staff ya lo vio/gestiono), Rejected=2.</summary>
    public int Status { get; set; } = 0;

    /// <summary>Mensaje opcional de la empresa al staff (a que puesto lo ven, urgencia...).</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
