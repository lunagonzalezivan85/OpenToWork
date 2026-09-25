namespace OpenToWork.Shared.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsMine { get; set; }
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantAvatar { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsRead { get; set; }
    public string? VacancyTitle { get; set; }
    public string? Subject { get; set; }
}

public class SendMessageDto
{
    /// <summary>Guid.Empty = iniciar una conversacion nueva con Trato Directo (usa Subject/VacancyId).</summary>
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public Guid? VacancyId { get; set; }
}

/// <summary>Conversacion vista desde la bandeja del admin (equipo de Trato Directo).</summary>
public class AdminConversationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Nombre del candidato o de la empresa.</summary>
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    /// <summary>UserRole: 0 = Candidato, 1 = Empresa.</summary>
    public int UserRole { get; set; }
    /// <summary>Id de la empresa (para enlazar a su ficha) cuando UserRole = Empresa.</summary>
    public Guid? CompanyId { get; set; }
    public string? Subject { get; set; }
    public string? VacancyTitle { get; set; }
    public string? AssignedStaffName { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class AdminMessageDto
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public bool IsFromStaff { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class AdminSendMessageDto
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>El equipo inicia una conversacion con un candidato o empresa (desde su ficha).</summary>
public class AdminStartConversationDto
{
    public Guid UserId { get; set; }
    public string? Subject { get; set; }
    public Guid? VacancyId { get; set; }
    public string Content { get; set; } = string.Empty;
}
