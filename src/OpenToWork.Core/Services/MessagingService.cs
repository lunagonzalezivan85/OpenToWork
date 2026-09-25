using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Mensajeria real (reemplaza los datos de ejemplo fijos de MessagesController). Solo usuario del
/// portal &lt;-&gt; equipo de Trato Directo: con el Embudo Ciego la empresa nunca habla directo con el
/// candidato (decision de Darwin, 25-Sep). El portal ve al otro lado siempre como "Trato Directo",
/// sin el nombre del reclutador.
/// </summary>
public class MessagingService : IMessagingService
{
    private const string TeamName = "Trato Directo";
    private const string TeamAvatar = "TD";
    private const int MaxLength = 4000;

    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly IAuditLogService _auditLog;

    public MessagingService(AppDbContext context, IEmailService email, IAuditLogService auditLog)
    {
        _context = context;
        _email = email;
        _auditLog = auditLog;
    }

    // ================= Portal =================

    public async Task<List<ConversationDto>> GetConversationsForUserAsync(Guid userId)
    {
        return await _context.PT_Conversations
            .Where(c => c.SCUserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                ParticipantName = TeamName,
                ParticipantAvatar = TeamAvatar,
                LastMessage = c.LastMessagePreview ?? string.Empty,
                LastMessageAt = c.LastMessageAt,
                UnreadCount = c.UnreadForUser,
                IsRead = c.UnreadForUser == 0,
                VacancyTitle = c.Vacancy != null ? c.Vacancy.Title : null,
                Subject = c.Subject
            })
            .ToListAsync();
    }

    public async Task<List<MessageDto>?> GetMessagesForUserAsync(Guid userId, Guid conversationId)
    {
        var owns = await _context.PT_Conversations
            .AnyAsync(c => c.Id == conversationId && c.SCUserId == userId && !c.IsDeleted);
        if (!owns) return null;

        return await _context.PT_Messages
            .Where(m => m.PT_ConversationId == conversationId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.PT_ConversationId,
                SenderName = m.IsFromStaff ? TeamName : "Tú",
                SenderAvatar = m.IsFromStaff ? TeamAvatar : "YO",
                Content = m.Content,
                SentAt = m.CreatedAt,
                IsRead = !m.IsFromStaff || m.ReadAt != null,
                IsMine = !m.IsFromStaff
            })
            .ToListAsync();
    }

    public async Task<MessageDto?> SendFromUserAsync(Guid userId, SendMessageDto dto)
    {
        var content = Normalize(dto.Content);

        PTConversation? conversation;
        if (dto.ConversationId == Guid.Empty)
        {
            var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null || (user.PrimaryRole != (int)UserRole.Candidate && user.PrimaryRole != (int)UserRole.Company))
                throw new InvalidOperationException("Solo candidatos y empresas pueden escribir a Trato Directo desde el portal.");

            conversation = new PTConversation
            {
                SCUserId = userId,
                Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim()[..Math.Min(dto.Subject.Trim().Length, 200)],
                PT_VacancyId = dto.VacancyId,
                CreatedBy = userId
            };
            _context.PT_Conversations.Add(conversation);
        }
        else
        {
            conversation = await _context.PT_Conversations
                .FirstOrDefaultAsync(c => c.Id == dto.ConversationId && c.SCUserId == userId && !c.IsDeleted);
            if (conversation == null) return null;
        }

        var message = AddMessage(conversation, userId, fromStaff: false, content);
        await _context.SaveChangesAsync();

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = conversation.Id,
            SenderName = "Tú",
            SenderAvatar = "YO",
            Content = message.Content,
            SentAt = message.CreatedAt,
            IsRead = true,
            IsMine = true
        };
    }

    public async Task<bool> MarkReadForUserAsync(Guid userId, Guid conversationId)
    {
        var conversation = await _context.PT_Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.SCUserId == userId && !c.IsDeleted);
        if (conversation == null) return false;

        await MarkReadAsync(conversation, readerIsStaff: false);
        return true;
    }

    // ================= Admin =================

    public async Task<List<AdminConversationDto>> GetInboxAsync(bool unreadOnly, string? search)
    {
        var query = _context.PT_Conversations.Where(c => !c.IsDeleted);
        if (unreadOnly)
            query = query.Where(c => c.UnreadForStaff > 0);

        var items = await ProjectForStaff(query.OrderByDescending(c => c.LastMessageAt)).Take(200).ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            items = items.Where(i => i.UserName.Contains(s, StringComparison.OrdinalIgnoreCase)
                || i.UserEmail.Contains(s, StringComparison.OrdinalIgnoreCase)
                || (i.Subject?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        return items;
    }

    public async Task<int> GetUnreadCountForStaffAsync() =>
        await _context.PT_Conversations.Where(c => !c.IsDeleted && c.UnreadForStaff > 0).CountAsync();

    public async Task<AdminConversationDto?> GetConversationForStaffAsync(Guid conversationId) =>
        await ProjectForStaff(_context.PT_Conversations.Where(c => c.Id == conversationId && !c.IsDeleted)).FirstOrDefaultAsync();

    public async Task<List<AdminMessageDto>?> GetMessagesForStaffAsync(Guid conversationId)
    {
        var conversation = await ProjectForStaff(_context.PT_Conversations.Where(c => c.Id == conversationId && !c.IsDeleted))
            .FirstOrDefaultAsync();
        if (conversation == null) return null;

        return await _context.PT_Messages
            .Where(m => m.PT_ConversationId == conversationId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new AdminMessageDto
            {
                Id = m.Id,
                SenderName = m.IsFromStaff ? m.Sender.Email : conversation.UserName,
                IsFromStaff = m.IsFromStaff,
                Content = m.Content,
                SentAt = m.CreatedAt,
                ReadAt = m.ReadAt
            })
            .ToListAsync();
    }

    public async Task<AdminMessageDto?> SendFromStaffAsync(Guid staffId, Guid conversationId, string content, string? ipAddress)
    {
        var text = Normalize(content);
        var conversation = await _context.PT_Conversations
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);
        if (conversation == null) return null;

        conversation.AssignedStaffId ??= staffId;
        var message = AddMessage(conversation, staffId, fromStaff: true, text);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "Messages.Reply", "PTConversation", conversation.Id, null, ipAddress);
        await NotifyUserAsync(conversation, text);

        var staffEmail = await _context.SC_Users.Where(u => u.Id == staffId).Select(u => u.Email).FirstOrDefaultAsync();
        return new AdminMessageDto
        {
            Id = message.Id,
            SenderName = staffEmail ?? TeamName,
            IsFromStaff = true,
            Content = message.Content,
            SentAt = message.CreatedAt
        };
    }

    public async Task<Guid> StartFromStaffAsync(Guid staffId, AdminStartConversationDto dto, string? ipAddress)
    {
        var text = Normalize(dto.Content);
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && !u.IsDeleted)
            ?? throw new InvalidOperationException("El usuario no existe.");
        if (user.PrimaryRole != (int)UserRole.Candidate && user.PrimaryRole != (int)UserRole.Company)
            throw new InvalidOperationException("Solo se puede escribir a candidatos y empresas.");

        var conversation = new PTConversation
        {
            SCUserId = user.Id,
            User = user,
            Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim()[..Math.Min(dto.Subject.Trim().Length, 200)],
            PT_VacancyId = dto.VacancyId,
            AssignedStaffId = staffId,
            CreatedBy = staffId
        };
        _context.PT_Conversations.Add(conversation);
        AddMessage(conversation, staffId, fromStaff: true, text);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "Messages.Start", "PTConversation", conversation.Id, null, ipAddress);
        await NotifyUserAsync(conversation, text);
        return conversation.Id;
    }

    public async Task<bool> MarkReadForStaffAsync(Guid conversationId)
    {
        var conversation = await _context.PT_Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);
        if (conversation == null) return false;

        await MarkReadAsync(conversation, readerIsStaff: true);
        return true;
    }

    // ================= Helpers =================

    private static string Normalize(string? content)
    {
        var text = content?.Trim() ?? string.Empty;
        if (text.Length == 0) throw new InvalidOperationException("El mensaje esta vacio.");
        if (text.Length > MaxLength) throw new InvalidOperationException($"El mensaje supera los {MaxLength} caracteres.");
        return text;
    }

    private PTMessage AddMessage(PTConversation conversation, Guid senderId, bool fromStaff, string content)
    {
        var message = new PTMessage
        {
            Conversation = conversation,
            SenderUserId = senderId,
            IsFromStaff = fromStaff,
            Content = content,
            CreatedBy = senderId
        };
        _context.PT_Messages.Add(message);

        conversation.LastMessageAt = message.CreatedAt;
        conversation.LastMessagePreview = content.Length > 300 ? content[..297] + "..." : content;
        conversation.UpdatedAt = DateTime.UtcNow;
        if (fromStaff) conversation.UnreadForUser++;
        else conversation.UnreadForStaff++;
        return message;
    }

    private async Task MarkReadAsync(PTConversation conversation, bool readerIsStaff)
    {
        // El lector marca como leidos los mensajes del otro lado.
        var pending = await _context.PT_Messages
            .Where(m => m.PT_ConversationId == conversation.Id && !m.IsDeleted && m.ReadAt == null && m.IsFromStaff != readerIsStaff)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var m in pending) m.ReadAt = now;

        if (readerIsStaff) conversation.UnreadForStaff = 0;
        else conversation.UnreadForUser = 0;
        await _context.SaveChangesAsync();
    }

    /// <summary>Correo al usuario cuando le escribe el equipo (si el SMTP esta activo). No bloquea:
    /// si falla o esta apagado, el mensaje igual queda en el portal.</summary>
    private async Task NotifyUserAsync(PTConversation conversation, string content)
    {
        var email = conversation.User?.Email
            ?? await _context.SC_Users.Where(u => u.Id == conversation.SCUserId).Select(u => u.Email).FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(email)) return;

        var preview = System.Net.WebUtility.HtmlEncode(content.Length > 500 ? content[..497] + "..." : content);
        var subject = string.IsNullOrWhiteSpace(conversation.Subject)
            ? "Tienes un mensaje nuevo de Trato Directo"
            : $"Trato Directo: {conversation.Subject}";
        var html = $@"<p>Tienes un mensaje nuevo de <strong>Trato Directo</strong>:</p>
            <blockquote style=""border-left:3px solid #ccc;padding-left:10px;color:#333"">{preview}</blockquote>
            <p>Entra al portal, seccion Mensajes, para responder.</p>";
        await _email.SendAsync(email, null, subject, html);
    }

    /// <summary>Nombre del otro lado para el admin: nombre del candidato o de la empresa.</summary>
    private IQueryable<AdminConversationDto> ProjectForStaff(IQueryable<PTConversation> query) =>
        query.Select(c => new AdminConversationDto
        {
            Id = c.Id,
            UserId = c.SCUserId,
            UserEmail = c.User.Email,
            UserRole = c.User.PrimaryRole,
            // Candidato sin nombre cargado (perfil a medias) -> su correo.
            UserName = c.User.Candidate != null && (c.User.Candidate.FirstName + " " + c.User.Candidate.LastName).Trim() != ""
                ? (c.User.Candidate.FirstName + " " + c.User.Candidate.LastName).Trim()
                : c.User.Company != null && c.User.Company.Name != "" ? c.User.Company.Name : c.User.Email,
            CompanyId = c.User.Company != null ? c.User.Company.Id : null,
            Subject = c.Subject,
            VacancyTitle = c.Vacancy != null ? c.Vacancy.Title : null,
            AssignedStaffName = c.AssignedStaff != null ? c.AssignedStaff.Email : null,
            LastMessage = c.LastMessagePreview ?? string.Empty,
            LastMessageAt = c.LastMessageAt,
            UnreadCount = c.UnreadForStaff
        });
}
