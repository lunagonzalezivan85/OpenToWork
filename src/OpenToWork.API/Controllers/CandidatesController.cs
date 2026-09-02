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
    private readonly ICandidateSearchService _candidateSearchService;

    public CandidatesController(ICandidateService candidateService, IValidationService validationService, IScoringService scoringService, IReferenceService referenceService, IVerificationStatusService verificationStatusService, ICandidateSearchService candidateSearchService)
    {
        _candidateService = candidateService;
        _validationService = validationService;
        _scoringService = scoringService;
        _referenceService = referenceService;
        _verificationStatusService = verificationStatusService;
        _candidateSearchService = candidateSearchService;
    }

    /// <summary>Busqueda avanzada de la empresa por score/verificacion/skill (Fase 5). Solo candidatos con perfil publico.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CandidateSearchFilterDto filter)
    {
        var result = await _candidateSearchService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>Skills que aparecen en candidatos con perfil publico - para el filtro de busqueda.</summary>
    [HttpGet("search/skills")]
    public async Task<IActionResult> GetSearchableSkills()
    {
        var result = await _candidateSearchService.GetSearchableSkillsAsync();
        return Ok(result);
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
    /// <summary>Lectura pura, no dispara HTTP (agregado en 3.8 para la seccion Verificaciones del dashboard).</summary>
    [HttpGet("{id}/verifications")]
    public async Task<IActionResult> GetVerifications(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var myCandidate = await _candidateService.GetCandidateByUserIdAsync(userId.Value);
        if (myCandidate == null || myCandidate.Id != id) return Forbid();

        var results = await _validationService.GetVerificationsAsync(id);
        return Ok(results);
    }

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
    /// Lectura pura, no recalcula. Visible para cualquier usuario autenticado (no solo el
    /// dueno) - mismo criterio ya usado por ProfileController.GetCandidateProfile para el
    /// perfil publico; la empresa lo necesita para busqueda avanzada y postulantes (Fase 5).
    /// </summary>
    [HttpGet("{id}/score")]
    public async Task<IActionResult> GetScore(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _scoringService.GetScoreAsync(id);
        return Ok(result);
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
    /// {id} es el Id de PTCandidate. Visible para cualquier usuario autenticado (no solo el
    /// dueno) desde Fase 5 - resuelve el gap de acceso dejado abierto en fase-3-sub7.md
    /// pregunta 6, mismo criterio que GetScore de arriba: la empresa necesita ver el badge
    /// "Verificado TD" de otros candidatos en postulantes y busqueda avanzada.
    /// </summary>
    [HttpGet("{id}/verification-status")]
    public async Task<IActionResult> GetVerificationStatus(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _verificationStatusService.GetVerificationStatusAsync(id);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
