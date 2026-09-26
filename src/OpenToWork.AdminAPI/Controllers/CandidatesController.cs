using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;
using System.Text;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/candidates")]
[RequireStaffRole(AdminStaffRole.Reclutador)]
public class CandidatesController : AdminControllerBase
{
    private readonly IAdminCandidateService _candidateService;
    private readonly IAdminCandidateRegistrationService _registrationService;
    private readonly ILinkedinSearchService _linkedinSearchService;
    private readonly IWebHostEnvironment _env;
    private readonly IScoringService _scoringService;
    private readonly IValidationService _validationService;
    private readonly IVerificationStatusService _verificationStatusService;
    private readonly IAdminApplicationService _applicationService;
    private readonly ICompatibilityService _compatibilityService;
    private readonly ICvStorage _cvStorage;
    private readonly IProfilePhotoStorage _photoStorage;
    private readonly IProfileService _profileService;

    public CandidatesController(
        IAdminCandidateService candidateService,
        IAdminCandidateRegistrationService registrationService,
        ILinkedinSearchService linkedinSearchService,
        IWebHostEnvironment env,
        IScoringService scoringService,
        IValidationService validationService,
        IVerificationStatusService verificationStatusService,
        IAdminApplicationService applicationService,
        ICompatibilityService compatibilityService,
        ICvStorage cvStorage,
        IProfilePhotoStorage photoStorage,
        IProfileService profileService)
    {
        _candidateService = candidateService;
        _registrationService = registrationService;
        _linkedinSearchService = linkedinSearchService;
        _env = env;
        _scoringService = scoringService;
        _validationService = validationService;
        _verificationStatusService = verificationStatusService;
        _applicationService = applicationService;
        _compatibilityService = compatibilityService;
        _cvStorage = cvStorage;
        _photoStorage = photoStorage;
        _profileService = profileService;
    }

    /// <summary>Foto de perfil del candidato para el equipo de TD (almacenamiento privado).</summary>
    [HttpGet("{userId:guid}/photo")]
    public async Task<IActionResult> GetPhoto(Guid userId)
    {
        var photo = _photoStorage.ResolveForOwner(await _profileService.GetProfilePictureAsync(userId), userId);
        return photo == null ? NotFound() : PhysicalFile(photo.Value.Path, photo.Value.ContentType);
    }

    /// <summary>CV del candidato para el equipo de TD (carpeta privada; antes era un archivo publico
    /// y el enlace relativo del admin apuntaba a un servidor que no lo tenia).</summary>
    [HttpGet("{userId:guid}/cv")]
    public async Task<IActionResult> GetCv(Guid userId)
    {
        var profile = await _profileService.GetProfileAsync(userId);
        var path = profile == null ? null : _cvStorage.ResolveForOwner(profile.CvUrl, userId);
        if (path == null) return NotFound();

        var name = $"{profile!.FirstName} {profile.LastName}".Trim();
        return PhysicalFile(path, "application/pdf", string.IsNullOrWhiteSpace(name) ? "CV.pdf" : $"CV {name}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? wizardCompleted = null,
        [FromQuery] bool? hasLinkedIn = null,
        [FromQuery] bool? hasPortfolio = null,
        [FromQuery] bool? hasCV = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? skillId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        [FromQuery] string? recruitmentStatus = null)
    {
        var result = await _candidateService.GetCandidatesAsync(
            page, pageSize, search, wizardCompleted, hasLinkedIn,
            hasPortfolio, hasCV, isActive, skillId, sortBy, sortDesc, recruitmentStatus);
        return Ok(result);
    }

    [HttpPost("bulk-activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActionDto dto)
    {
        var result = await _candidateService.BulkActivateAsync(dto.Ids, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("bulk-deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActionDto dto)
    {
        var result = await _candidateService.BulkDeactivateAsync(dto.Ids, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var csv = await _candidateService.ExportCandidatesCsvAsync();
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"candidates-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpPost("register-cv")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> RegisterFromCv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se subió ningún archivo");

        var allowedTypes = new[] { "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest("Solo se permiten archivos PDF");

        if (file.Length > 10_000_000)
            return BadRequest("El archivo debe ser menor a 10MB");

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var result = await _registrationService.RegisterFromCvAsync(fileBytes, file.FileName, file.ContentType, AdminId);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        // Carpeta privada (ICvStorage), ya no wwwroot publico.
        if (!string.IsNullOrEmpty(result.CvUrl))
            await _cvStorage.SaveAsync(Path.GetFileName(result.CvUrl), fileBytes);

        return Ok(result);
    }

    [HttpPost("register-manual")]
    public async Task<IActionResult> RegisterManual([FromBody] AdminRegisterCandidateManualDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Email y contraseña son obligatorios");

        if (dto.Password.Length < 6)
            return BadRequest("La contraseña debe tener al menos 6 caracteres");

        var result = await _registrationService.RegisterManualAsync(dto, AdminId);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        return Ok(result);
    }

    [HttpPost("search-linkedin")]
    public async Task<IActionResult> SearchLinkedin([FromBody] LinkedinSearchRequestDto request)
    {
        var result = await _linkedinSearchService.SearchAsync(request.Country, request.City, request.Position);
        if (!result.Success)
            return BadRequest(new { error = result.Error });
        return Ok(result);
    }

    // --- Fase 3, sub-fase 3.8: Gestion de scores + Verificaciones manuales ---
    // {candidateId} es el Id de PTCandidate (AdminUserProfileDto.CandidateId), no el SCUserId.

    [HttpGet("{candidateId}/score")]
    public async Task<IActionResult> GetScore(Guid candidateId)
    {
        var result = await _scoringService.GetScoreAsync(candidateId);
        return Ok(result);
    }

    [HttpPost("{candidateId}/score/recalculate")]
    public async Task<IActionResult> RecalculateScore(Guid candidateId)
    {
        var result = await _scoringService.RecalculateAsync(candidateId);
        return Ok(result);
    }

    [HttpGet("{candidateId}/verifications")]
    public async Task<IActionResult> GetVerifications(Guid candidateId)
    {
        var result = await _validationService.GetVerificationsAsync(candidateId);
        return Ok(result);
    }

    /// <summary>Estado compuesto "Verificado TD" (mismo calculo que usan el dashboard del
    /// candidato y el portal de empresa) - antes solo se veia fuera del admin.</summary>
    [HttpGet("{candidateId}/verification-status")]
    public async Task<IActionResult> GetVerificationStatus(Guid candidateId)
    {
        try
        {
            var result = await _verificationStatusService.GetVerificationStatusAsync(candidateId);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>Aprobar/rechazar una verificacion manualmente ("verificaciones manuales" - item de Fase 4 desbloqueado desde fase-3-sub1.md).</summary>
    [HttpPut("{candidateId}/verifications/{type}")]
    public async Task<IActionResult> SetVerificationStatus(Guid candidateId, int type, [FromBody] SetVerificationStatusDto dto)
    {
        var result = await _validationService.SetVerificationStatusAsync(candidateId, type, dto.Status, AdminId);
        return Ok(result);
    }

    /// <summary>Postulaciones del candidato (tab "Vacantes" del perfil admin).</summary>
    [HttpGet("{candidateId}/applications")]
    public async Task<IActionResult> GetApplications(Guid candidateId)
    {
        var result = await _applicationService.GetByCandidateAsync(candidateId);
        return Ok(result);
    }

    /// <summary>Vacantes recomendadas por match calculado (tab "Vacantes" del perfil admin).</summary>
    [HttpGet("{candidateId}/matches")]
    public async Task<IActionResult> GetMatches(Guid candidateId)
    {
        var result = await _compatibilityService.GetMatchesByCandidateAsync(candidateId);
        return Ok(result);
    }
}

public class SetVerificationStatusDto
{
    public int Status { get; set; }
}

public class BulkActionDto
{
    public List<Guid> Ids { get; set; } = new();
}
