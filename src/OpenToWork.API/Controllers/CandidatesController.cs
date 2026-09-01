using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly IValidationService _validationService;
    private readonly IScoringService _scoringService;

    public CandidatesController(ICandidateService candidateService, IValidationService validationService, IScoringService scoringService)
    {
        _candidateService = candidateService;
        _validationService = validationService;
        _scoringService = scoringService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var candidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (candidate == null)
        {
            candidate = await _candidateService.CreateCandidateAsync(userId.Value, userId.Value.ToString());
        }

        return Ok(candidate);
    }

    [HttpGet("wizard-status")]
    public async Task<IActionResult> GetWizardStatus()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var isComplete = await _candidateService.IsWizardCompleteAsync(userId.Value);
        return Ok(new { wizardCompleted = isComplete });
    }

    [HttpPut("wizard")]
    public async Task<IActionResult> UpdateWizard([FromBody] UpdateCandidateWizardDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _candidateService.UpdateWizardStepAsync(userId.Value, dto);
        return Ok(result);
    }

    /// <summary>
    /// {id} es el Id de PTCandidate (no el SCUserId). Solo el dueno del perfil puede
    /// disparar sus propias verificaciones - evita que cualquier usuario autenticado gatille
    /// verificaciones (y las peticiones HTTP salientes que implican) contra otro candidato.
    /// </summary>
    [HttpPost("{id}/verifications/run")]
    public async Task<IActionResult> RunVerifications(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var results = await _validationService.RunAllVerificationsAsync(id);
        return Ok(results);
    }

    /// <summary>
    /// {id} es el Id de PTCandidate (no el SCUserId). Solo el dueno del perfil puede recalcular
    /// su propio score (mismo guard de ownership que /verifications/run).
    /// </summary>
    [HttpPost("{id}/score/recalculate")]
    public async Task<IActionResult> RecalculateScore(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var result = await _scoringService.RecalculateAsync(id);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
