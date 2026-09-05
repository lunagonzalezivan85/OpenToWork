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

    public virtual ICollection<PTNegotiationCandidate> Candidates { get; set; } = new List<PTNegotiationCandidate>();
}
