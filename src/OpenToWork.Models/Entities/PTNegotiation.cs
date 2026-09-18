using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Ronda de negociacion: candidatos del shortlist presentados a la empresa para una vacante,
/// hasta que se elige un ganador (o se cancela). Cerrar la negociacion es lo que efectivamente
/// marca la vacante como cubierta.
/// </summary>
public class PTNegotiation : BaseEntity
{
    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    public int Status { get; set; } = 0;

    public Guid? AssignedStaffId { get; set; }

    [ForeignKey("AssignedStaffId")]
    public virtual SCUser? AssignedStaff { get; set; }

    public DateTime? PresentedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public Guid? WinningApplicationId { get; set; }

    [ForeignKey("WinningApplicationId")]
    public virtual PTApplication? WinningApplication { get; set; }

    public string? Notes { get; set; }

    /// <summary>Paso 16 del flujo comercial: fecha en que la empresa contrata formalmente al
    /// candidato (firma su contrato laboral). TD no formaliza ni gestiona ese contrato - es
    /// exclusivamente entre la empresa y el candidato - pero registra la fecha como hito del
    /// proceso. Independiente de IncorporationDate (que es el primer dia de trabajo).</summary>
    public DateTime? HiringDate { get; set; }

    /// <summary>Fecha en que el candidato ganador inicio funciones en la empresa (paso 17 del
    /// flujo comercial). Solo tiene sentido cuando Status = Cerrada. La usa el calculo de
    /// garantia (WarrantyDays del contrato + esta fecha).</summary>
    public DateTime? IncorporationDate { get; set; }

    /// <summary>Paso 21 del flujo comercial: cierre administrativo del proceso una vez vencida
    /// la garantia (o sin garantia definida) y sin reposiciones sin resolver. Distingue un
    /// "cerrado exitoso post-garantia" de simplemente Status=Cerrada.</summary>
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

    public virtual ICollection<PTNegotiationCandidate> Candidates { get; set; } = new List<PTNegotiationCandidate>();
}
