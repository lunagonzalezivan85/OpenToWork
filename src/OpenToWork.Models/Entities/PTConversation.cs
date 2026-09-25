using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Conversacion entre un usuario del portal (candidato o empresa) y el equipo de Trato Directo.
/// Nunca entre empresa y candidato directamente: con el Embudo Ciego TD es el intermediario
/// (decision de Darwin, 25-Sep). Los contadores de no leidos se mantienen desnormalizados para
/// listar las bandejas sin contar mensajes.
/// </summary>
public class PTConversation : BaseEntity
{
    /// <summary>Usuario del portal (candidato o empresa) dueño de la conversacion.</summary>
    [Required]
    public Guid SCUserId { get; set; }

    [ForeignKey("SCUserId")]
    public virtual SCUser User { get; set; } = null!;

    /// <summary>Vacante de referencia (opcional).</summary>
    public Guid? PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy? Vacancy { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    /// <summary>Miembro del equipo que la atiende (el primero que responde, o quien la inicio).</summary>
    public Guid? AssignedStaffId { get; set; }

    [ForeignKey("AssignedStaffId")]
    public virtual SCUser? AssignedStaff { get; set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? LastMessagePreview { get; set; }

    public int UnreadForUser { get; set; }

    public int UnreadForStaff { get; set; }

    public virtual ICollection<PTMessage> Messages { get; set; } = new List<PTMessage>();
}
