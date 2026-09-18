using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCandidateDelivery : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    [Required]
    public Guid DeliveredByUserId { get; set; }

    [ForeignKey("DeliveredByUserId")]
    public virtual SCUser DeliveredByUser { get; set; } = null!;

    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    public int Status { get; set; } = (int)Shared.Enums.DeliveryStatus.Delivered;

    [MaxLength(1000)]
    public string? AdminNote { get; set; }

    [MaxLength(1000)]
    public string? CompanyFeedback { get; set; }

    public DateTime? ViewedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    /// <summary>Paso 16 del flujo comercial: fecha en que la empresa contrata formalmente al
    /// candidato (firma su contrato laboral). TD no formaliza ni gestiona ese contrato - es
    /// exclusivamente entre la empresa y el candidato - pero registra la fecha como hito del
    /// proceso. Independiente de IncorporationDate (que es el primer dia de trabajo).</summary>
    public DateTime? HiringDate { get; set; }

    /// <summary>Fecha en que el candidato inicio funciones en la empresa (paso 17 del flujo
    /// comercial). Solo tiene sentido cuando Status = Hired. La usa el calculo de garantia
    /// (WarrantyDays del contrato de la vacante + esta fecha).</summary>
    public DateTime? IncorporationDate { get; set; }

    /// <summary>Paso 21 del flujo comercial: cierre administrativo del proceso una vez vencida
    /// la garantia (o sin garantia definida) y sin reposiciones sin resolver. Distingue un
    /// "cerrado exitoso post-garantia" de simplemente Status=Hired.</summary>
    public DateTime? ProcessClosedAt { get; set; }

    public Guid? ProcessClosedByUserId { get; set; }

    [ForeignKey("ProcessClosedByUserId")]
    public virtual SCUser? ProcessClosedByUser { get; set; }

    public string? ProcessClosureNotes { get; set; }

    /// <summary>Paso 22: feedback de mejora continua registrado por el admin (conversacion con
    /// el cliente) una vez cerrado el proceso. Rating 1-5, null si no se ha registrado.</summary>
    public int? FeedbackRating { get; set; }

    public string? FeedbackComments { get; set; }

    public DateTime? FeedbackRecordedAt { get; set; }

    public Guid? FeedbackRecordedByUserId { get; set; }

    [ForeignKey("FeedbackRecordedByUserId")]
    public virtual SCUser? FeedbackRecordedByUser { get; set; }
}
