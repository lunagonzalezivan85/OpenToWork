using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>Mensajeria entre usuarios del portal (candidato/empresa) y el equipo de Trato Directo.</summary>
public interface IMessagingService
{
    // --- Portal (usuario candidato o empresa) ---
    Task<List<ConversationDto>> GetConversationsForUserAsync(Guid userId);
    /// <summary>Null si la conversacion no existe o no es de este usuario.</summary>
    Task<List<MessageDto>?> GetMessagesForUserAsync(Guid userId, Guid conversationId);
    /// <summary>ConversationId vacio = inicia una conversacion nueva con Trato Directo.</summary>
    Task<MessageDto?> SendFromUserAsync(Guid userId, SendMessageDto dto);
    Task<bool> MarkReadForUserAsync(Guid userId, Guid conversationId);

    // --- Admin (equipo de Trato Directo) ---
    Task<List<AdminConversationDto>> GetInboxAsync(bool unreadOnly, string? search);
    Task<int> GetUnreadCountForStaffAsync();
    Task<AdminConversationDto?> GetConversationForStaffAsync(Guid conversationId);
    Task<List<AdminMessageDto>?> GetMessagesForStaffAsync(Guid conversationId);
    Task<AdminMessageDto?> SendFromStaffAsync(Guid staffId, Guid conversationId, string content, string? ipAddress);
    Task<Guid> StartFromStaffAsync(Guid staffId, AdminStartConversationDto dto, string? ipAddress);
    Task<bool> MarkReadForStaffAsync(Guid conversationId);
}
