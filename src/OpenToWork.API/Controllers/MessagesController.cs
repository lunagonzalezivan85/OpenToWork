using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var conversations = new List<ConversationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ParticipantName = "TechCorp Solutions",
                ParticipantAvatar = "TC",
                LastMessage = "Hola, hemos revisado tu perfil y nos gustar\u00eda invitarte a una entrevista.",
                LastMessageAt = DateTime.UtcNow.AddHours(-2),
                UnreadCount = 2,
                IsRead = false,
                VacancyTitle = "Desarrollador Frontend Senior"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ParticipantName = "Innovate Labs",
                ParticipantAvatar = "IL",
                LastMessage = "\u00a1Gracias por tu postulaci\u00f3n! Te contactaremos pronto.",
                LastMessageAt = DateTime.UtcNow.AddHours(-5),
                UnreadCount = 0,
                IsRead = true,
                VacancyTitle = "Full Stack Developer"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ParticipantName = "GlobalSoft Inc.",
                ParticipantAvatar = "GS",
                LastMessage = "Podr\u00edas enviarnos tu portafolio?",
                LastMessageAt = DateTime.UtcNow.AddDays(-1),
                UnreadCount = 1,
                IsRead = false,
                VacancyTitle = "Dise\u00f1ador UX/UI"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ParticipantName = "Maria Gonzalez",
                ParticipantAvatar = "MG",
                LastMessage = "Nos encant\u00f3 tu presentaci\u00f3n en video. \u00a1Felicidades!",
                LastMessageAt = DateTime.UtcNow.AddDays(-3),
                UnreadCount = 0,
                IsRead = true,
                VacancyTitle = null
            }
        };

        return Ok(conversations);
    }

    [HttpGet("{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var messages = new List<MessageDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderName = "TechCorp Solutions",
                SenderAvatar = "TC",
                Content = "Hola, hemos revisado tu perfil y nos gustar\u00eda invitarte a una entrevista.",
                SentAt = DateTime.UtcNow.AddHours(-3),
                IsRead = true,
                IsMine = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderName = "T\u00fa",
                SenderAvatar = "YO",
                Content = "\u00a1Hola! Muchas gracias por la oportunidad. Estoy muy interesado en la posici\u00f3n.",
                SentAt = DateTime.UtcNow.AddHours(-2.5),
                IsRead = true,
                IsMine = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderName = "TechCorp Solutions",
                SenderAvatar = "TC",
                Content = "\u00bfQu\u00e9 d\u00eda te vendr\u00eda bien para la entrevista? Tenemos disponibilidad ma\u00f1ana o el jueves.",
                SentAt = DateTime.UtcNow.AddHours(-2),
                IsRead = false,
                IsMine = false
            }
        };

        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Message content cannot be empty");

        var message = new MessageDto
        {
            Id = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            SenderName = "T\u00fa",
            SenderAvatar = "YO",
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            IsRead = true,
            IsMine = true
        };

        return Ok(message);
    }

    [HttpPut("{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        return Ok();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
