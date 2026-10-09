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
    private readonly IProfileService _profileService;
    private readonly ISystemConfigService _systemConfig;

    public CandidatesController(ICandidateService candidateService, IValidationService validationService, IScoringService scoringService, IReferenceService referenceService, IVerificationStatusService verificationStatusService, IProfileService profileService, ISystemConfigService systemConfig)
    {
        _candidateService = candidateService;
        _validationService = validationService;
        _scoringService = scoringService;
        _referenceService = referenceService;
        _verificationStatusService = verificationStatusService;
        _profileService = profileService;
        _systemConfig = systemConfig;
    }

    // Busqueda de candidatos por empresas: CERRADA (auditoria 8-Oct, H-39). Cualquier usuario con sesion,
    // incluso una empresa sin verificar o un candidato, listaba a candidatos reales con nombre y score.
    // Se reabrira solo con verificacion de empresas, opt-in del candidato y datos minimos
    // (docs/dsiezar/seguridad-busqueda-candidatos.md). ICandidateSearchService se conserva para ese rediseno.
    [HttpGet("search")]
    public IActionResult Search() => SearchClosed();

    [HttpGet("search/skills")]
    public IActionResult GetSearchableSkills() => SearchClosed();

    private IActionResult SearchClosed() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "La busqueda de candidatos no esta disponible." });

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

    /// <summary>Proceso del propio candidato: etapa, empresas a las que se le presento y plan (/my-process).</summary>
    [HttpGet("me/process")]
    public async Task<IActionResult> GetMyProcess()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var process = await _candidateService.GetMyProcessAsync(userId.Value);
        process.PlanFeatureEnabled = await _systemConfig.GetCandidatePriorityPlanEnabledAsync();
        return Ok(process);
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
    /// Lectura pura, no recalcula. Solo el propio candidato o una empresa a la que TD se lo
    /// entrego (misma regla que el perfil y el CV). Antes lo veia cualquier usuario con sesion
    /// (auditoria 8-Oct, H-39/H-27).
    /// </summary>
    [HttpGet("{id}/score")]
    public async Task<IActionResult> GetScore(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await _profileService.CanViewCandidateAsync(id, userId.Value)) return NotFound();

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

        try
        {
            var result = await _referenceService.AddReferenceAsync(id, dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// {id} es el Id de PTCandidate. Mismo criterio que GetScore: el propio candidato o una
    /// empresa a la que TD se lo entrego.
    /// </summary>
    [HttpGet("{id}/verification-status")]
    public async Task<IActionResult> GetVerificationStatus(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await _profileService.CanViewCandidateAsync(id, userId.Value)) return NotFound();

        var result = await _verificationStatusService.GetVerificationStatusAsync(id);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
