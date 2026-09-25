using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>Mensaje de una PTConversation (usuario del portal &lt;-&gt; equipo de Trato Directo).</summary>
public class PTMessage : BaseEntity
{
    [Required]
    public Guid PT_ConversationId { get; set; }

    [ForeignKey("PT_ConversationId")]
    public virtual PTConversation Conversation { get; set; } = null!;

    [Required]
    public Guid SenderUserId { get; set; }

    [ForeignKey("SenderUserId")]
    public virtual SCUser Sender { get; set; } = null!;

    /// <summary>true = lo escribio el equipo de Trato Directo (admin); false = el usuario del portal.</summary>
    public bool IsFromStaff { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Cuando lo leyo el destinatario (null = no leido).</summary>
    public DateTime? ReadAt { get; set; }
}
