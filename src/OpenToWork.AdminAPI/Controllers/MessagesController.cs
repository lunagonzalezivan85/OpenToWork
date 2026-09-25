using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>Bandeja de mensajes del equipo de Trato Directo con candidatos y empresas del portal.</summary>
[Route("api/admin/messages")]
public class MessagesController : AdminControllerBase
{
    private readonly IMessagingService _messaging;

    public MessagesController(IMessagingService messaging)
    {
        _messaging = messaging;
    }

    [HttpGet]
    public async Task<IActionResult> Inbox([FromQuery] bool unreadOnly = false, [FromQuery] string? search = null)
    {
        return Ok(await _messaging.GetInboxAsync(unreadOnly, search));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        return Ok(await _messaging.GetUnreadCountForStaffAsync());
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> Conversation(Guid conversationId)
    {
        var conversation = await _messaging.GetConversationForStaffAsync(conversationId);
        return conversation == null ? NotFound() : Ok(conversation);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> Messages(Guid conversationId)
    {
        var messages = await _messaging.GetMessagesForStaffAsync(conversationId);
        return messages == null ? NotFound() : Ok(messages);
    }

    [HttpPost("{conversationId:guid}/reply")]
    public async Task<IActionResult> Reply(Guid conversationId, [FromBody] AdminSendMessageDto dto)
    {
        try
        {
            var message = await _messaging.SendFromStaffAsync(AdminId, conversationId, dto.Content, ClientIp);
            return message == null ? NotFound() : Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Iniciar una conversacion con un candidato o empresa (desde su ficha).</summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] AdminStartConversationDto dto)
    {
        try
        {
            var id = await _messaging.StartFromStaffAsync(AdminId, dto, ClientIp);
            return Ok(new { conversationId = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid conversationId)
    {
        return await _messaging.MarkReadForStaffAsync(conversationId) ? Ok() : NotFound();
    }
}
