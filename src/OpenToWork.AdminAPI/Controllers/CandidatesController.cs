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

    public CandidatesController(
        IAdminCandidateService candidateService,
        IAdminCandidateRegistrationService registrationService,
        ILinkedinSearchService linkedinSearchService,
        IWebHostEnvironment env)
    {
        _candidateService = candidateService;
        _registrationService = registrationService;
        _linkedinSearchService = linkedinSearchService;
        _env = env;
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
}

public class BulkActionDto
{
    public List<Guid> Ids { get; set; } = new();
}
