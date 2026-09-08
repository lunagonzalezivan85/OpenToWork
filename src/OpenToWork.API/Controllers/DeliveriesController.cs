using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> My([FromQuery] Guid? vacancyId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _deliveryService.GetMyDeliveriesAsync(userId.Value, vacancyId);
        return Ok(result);
    }

    [HttpGet("vacancy-summary/{vacancyId}")]
    public async Task<IActionResult> VacancySummary(Guid vacancyId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _deliveryService.GetVacancySummaryAsync(vacancyId, userId.Value);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id}/respond")]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondDeliveryDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _deliveryService.RespondToDeliveryAsync(id, dto.Status, dto.Feedback, userId.Value);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
