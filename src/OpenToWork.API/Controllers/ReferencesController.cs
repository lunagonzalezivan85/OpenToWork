using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/references")]
public class ReferencesController : ControllerBase
{
    private readonly IReferenceService _referenceService;
    private readonly ICandidateService _candidateService;

    public ReferencesController(IReferenceService referenceService, ICandidateService candidateService)
    {
        _referenceService = referenceService;
        _candidateService = candidateService;
    }

    /// <summary>Solo el dueno de la referencia puede disparar el envio de su propio link.</summary>
    [Authorize]
    [HttpPost("{id}/send")]
    public async Task<IActionResult> Send(Guid id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId);
        if (myCandidate == null) return Forbid();

        var result = await _referenceService.SendReferenceRequestAsync(myCandidate.Id, id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Publico - el contacto de la referencia no tiene cuenta en OpenToWork (fase-3-sub5.md
    /// pregunta 3). El token en el body (no en la URL) identifica la referencia, mismo patron
    /// que POST api/auth/reset-password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback([FromBody] SubmitReferenceFeedbackDto dto)
    {
        var ok = await _referenceService.SubmitReferenceFeedbackAsync(dto.Token, dto.Rating, dto.Feedback);
        return ok ? NoContent() : BadRequest(new { message = "Token invalido, vencido, o referencia ya respondida." });
    }
}
