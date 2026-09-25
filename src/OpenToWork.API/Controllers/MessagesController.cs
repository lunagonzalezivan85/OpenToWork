using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

/// <summary>Mensajes del usuario del portal (candidato o empresa) con el equipo de Trato Directo.
/// Antes devolvia conversaciones de ejemplo fijas; ahora lee y guarda en PT_Conversations/PT_Messages.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messaging;

    public MessagesController(IMessagingService messaging)
    {
        _messaging = messaging;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return Ok(await _messaging.GetConversationsForUserAsync(userId.Value));
    }

    [HttpGet("{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var messages = await _messaging.GetMessagesForUserAsync(userId.Value, conversationId);
        return messages == null ? NotFound() : Ok(messages);
    }

    /// <summary>ConversationId vacio = inicia una conversacion nueva con Trato Directo.</summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var message = await _messaging.SendFromUserAsync(userId.Value, dto);
            return message == null ? NotFound() : Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        return await _messaging.MarkReadForUserAsync(userId.Value, conversationId) ? Ok() : NotFound();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
