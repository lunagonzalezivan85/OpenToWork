using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Shared.DTOs;
using System.Text;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/candidates")]
public class CandidatesController : AdminControllerBase
{
    private readonly IAdminCandidateService _candidateService;
    private readonly IAdminCandidateRegistrationService _registrationService;
    private readonly ILinkedinSearchService _linkedinSearchService;
    private readonly IWebHostEnvironment _env;
    private readonly IScoringService _scoringService;
    private readonly IValidationService _validationService;

    public CandidatesController(
        IAdminCandidateService candidateService,
        IAdminCandidateRegistrationService registrationService,
        ILinkedinSearchService linkedinSearchService,
        IWebHostEnvironment env,
        IScoringService scoringService,
        IValidationService validationService)
    {
        _candidateService = candidateService;
        _registrationService = registrationService;
        _linkedinSearchService = linkedinSearchService;
        _env = env;
        _scoringService = scoringService;
        _validationService = validationService;
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

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "cv");
        Directory.CreateDirectory(uploadsDir);

        var result = await _registrationService.RegisterFromCvAsync(fileBytes, file.FileName, file.ContentType, AdminId);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        if (!string.IsNullOrEmpty(result.CvUrl))
        {
            var fileName = Path.GetFileName(result.CvUrl);
            var filePath = Path.Combine(uploadsDir, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }

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

    /// <summary>Aprobar/rechazar una verificacion manualmente ("verificaciones manuales" - item de Fase 4 desbloqueado desde fase-3-sub1.md).</summary>
    [HttpPut("{candidateId}/verifications/{type}")]
    public async Task<IActionResult> SetVerificationStatus(Guid candidateId, int type, [FromBody] SetVerificationStatusDto dto)
    {
        var result = await _validationService.SetVerificationStatusAsync(candidateId, type, dto.Status, AdminId);
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
