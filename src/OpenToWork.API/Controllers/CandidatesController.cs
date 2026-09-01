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
    private readonly IReferenceService _referenceService;
    private readonly IVerificationStatusService _verificationStatusService;

    public CandidatesController(ICandidateService candidateService, IValidationService validationService, IScoringService scoringService, IReferenceService referenceService, IVerificationStatusService verificationStatusService)
    {
        _candidateService = candidateService;
        _validationService = validationService;
        _scoringService = scoringService;
        _referenceService = referenceService;
        _verificationStatusService = verificationStatusService;
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

    /// <summary>{id} es el Id de PTCandidate. Solo el dueno del perfil ve sus propias referencias.</summary>
    [HttpGet("{id}/references")]
    public async Task<IActionResult> GetReferences(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var result = await _referenceService.GetReferencesAsync(id);
        return Ok(result);
    }

    /// <summary>{id} es el Id de PTCandidate. Solo el dueno del perfil agrega sus propias referencias.</summary>
    [HttpPost("{id}/references")]
    public async Task<IActionResult> AddReference(Guid id, [FromBody] CreateReferenceDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var result = await _referenceService.AddReferenceAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// {id} es el Id de PTCandidate. Solo el dueno del perfil por ahora - la exposicion
    /// publica/para empresas del distintivo Verificado TD se resuelve en la sub-fase 3.8, junto
    /// con el modelo de autenticacion de empresa (fase-3-sub7.md pregunta 6).
    /// </summary>
    [HttpGet("{id}/verification-status")]
    public async Task<IActionResult> GetVerificationStatus(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var result = await _verificationStatusService.GetVerificationStatusAsync(id);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
