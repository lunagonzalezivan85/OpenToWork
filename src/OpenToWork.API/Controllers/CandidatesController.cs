using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
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
    private readonly ICandidateSearchService _candidateSearchService;
    private readonly ISystemConfigService _systemConfig;
    private readonly AppDbContext _context;

    public CandidatesController(ICandidateService candidateService, IValidationService validationService, IScoringService scoringService, IReferenceService referenceService, IVerificationStatusService verificationStatusService, IProfileService profileService, ICandidateSearchService candidateSearchService, ISystemConfigService systemConfig, AppDbContext context)
    {
        _candidateService = candidateService;
        _validationService = validationService;
        _scoringService = scoringService;
        _referenceService = referenceService;
        _verificationStatusService = verificationStatusService;
        _profileService = profileService;
        _candidateSearchService = candidateSearchService;
        _systemConfig = systemConfig;
        _context = context;
    }

    // Busqueda de candidatos (auditoria 8-Oct, H-39): reabierta con las tres condiciones del
    // rediseno - empresa verificada por el staff (403 al resto), candidatos solo si dieron
    // consentimiento de visibilidad (VisibilityConsentAt) y datos minimos + apellido
    // enmascarado en el resultado. La identidad completa sigue detras de la entrega.
    /// <summary>Solo una empresa verificada por el equipo TD puede buscar candidatos.
    /// Usuarios sin empresa (candidatos) o con empresa sin verificar -> 403.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] CandidateSearchFilterDto filter)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await IsVerifiedCompanyAsync(userId.Value)) return Forbid();

        var result = await _candidateSearchService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>Skills que aparecen en candidatos visibles - para el filtro de busqueda.
    /// Misma regla que search: solo empresas verificadas.</summary>
    [HttpGet("search/skills")]
    public async Task<IActionResult> GetSearchableSkills()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await IsVerifiedCompanyAsync(userId.Value)) return Forbid();

        var result = await _candidateSearchService.GetSearchableSkillsAsync();
        return Ok(result);
    }

    private Task<bool> IsVerifiedCompanyAsync(Guid userId) =>
        _context.PT_Companies.AnyAsync(c => c.SCUserId == userId && !c.IsDeleted && c.IsVerified);

    // --- Solicitudes de candidato (empresa verificada -> staff): senal para que TD prepare
    // la entrega por el pipeline existente. NO desbloquea nada por si sola. ---

    /// <summary>Empresa verificada solicita que TD le presente a este candidato.</summary>
    [HttpPost("{id}/request")]
    public async Task<IActionResult> RequestCandidate(Guid id, [FromBody] CandidateRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.SCUserId == userId.Value && !c.IsDeleted && c.IsVerified);
        if (company == null) return Forbid();

        var candidateVisible = await _context.PT_Candidates
            .AnyAsync(c => c.Id == id && !c.IsDeleted && c.IsProfilePublic && c.WizardCompleted
                && c.VisibilityConsentAt != null);
        if (!candidateVisible) return NotFound();

        if (dto.VacancyId.HasValue)
        {
            var vacancyOwned = await _context.PT_Vacancies
                .AnyAsync(v => v.Id == dto.VacancyId.Value && !v.IsDeleted && v.PT_CompanyId == company.Id);
            if (!vacancyOwned) return BadRequest(new { message = "vacancy_not_owned" });
        }

        var duplicate = await _context.PT_CandidateRequests
            .AnyAsync(r => r.PT_CompanyId == company.Id && r.PT_CandidateId == id
                && r.Status == 0 && !r.IsDeleted);
        if (duplicate) return Conflict(new { message = "already_requested" });

        _context.PT_CandidateRequests.Add(new PTCandidateRequest
        {
            PT_CompanyId = company.Id,
            RequestedByUserId = userId.Value,
            PT_CandidateId = id,
            PT_VacancyId = dto.VacancyId,
            Notes = dto.Notes
        });
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Candidatos que esta empresa ya ha solicitado (para marcar "solicitado" en la lista).</summary>
    [HttpGet("company/requests")]
    public async Task<IActionResult> GetCompanyRequests()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.SCUserId == userId.Value && !c.IsDeleted);
        if (company == null) return Ok(Array.Empty<Guid>());

        var ids = await _context.PT_CandidateRequests
            .Where(r => r.PT_CompanyId == company.Id && !r.IsDeleted && r.Status == 0)
            .Select(r => r.PT_CandidateId)
            .ToListAsync();
        return Ok(ids);
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
