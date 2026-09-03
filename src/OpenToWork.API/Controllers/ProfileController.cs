using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ICvParserService _cvParserService;
    private readonly IWebHostEnvironment _env;

    public ProfileController(IProfileService profileService, ICvParserService cvParserService, IWebHostEnvironment env)
    {
        _profileService = profileService;
        _cvParserService = cvParserService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.GetProfileAsync(userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetCandidateProfile(Guid candidateId)
    {
        var result = await _profileService.GetCandidateByIdAsync(candidateId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.UpdateProfileAsync(userId.Value, dto);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("experience")]
    public async Task<IActionResult> AddExperience([FromBody] CreateExperienceDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.AddExperienceAsync(userId.Value, dto);
        return Ok(result);
    }

    [HttpPut("experience/{id}")]
    public async Task<IActionResult> UpdateExperience(Guid id, [FromBody] UpdateExperienceDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.UpdateExperienceAsync(id, dto, userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("experience/{id}")]
    public async Task<IActionResult> DeleteExperience(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _profileService.DeleteExperienceAsync(id, userId.Value);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation([FromBody] CreateEducationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.AddEducationAsync(userId.Value, dto);
        return Ok(result);
    }

    [HttpPut("education/{id}")]
    public async Task<IActionResult> UpdateEducation(Guid id, [FromBody] UpdateEducationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.UpdateEducationAsync(id, dto, userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("education/{id}")]
    public async Task<IActionResult> DeleteEducation(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _profileService.DeleteEducationAsync(id, userId.Value);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("certification")]
    public async Task<IActionResult> AddCertification([FromBody] CreateCertificationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.AddCertificationAsync(userId.Value, dto);
        return Ok(result);
    }

    [HttpPut("certification/{id}")]
    public async Task<IActionResult> UpdateCertification(Guid id, [FromBody] UpdateCertificationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _profileService.UpdateCertificationAsync(id, dto, userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("certification/{id}")]
    public async Task<IActionResult> DeleteCertification(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _profileService.DeleteCertificationAsync(id, userId.Value);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("upload-cv")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadCv(IFormFile file)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedTypes = new[] { "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest("Only PDF files are allowed");

        if (file.Length > 10_000_000)
            return BadRequest("File size must be less than 10MB");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "cv");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"cv_{userId.Value}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var cvUrl = $"/uploads/cv/{fileName}";

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        CvParseResultDto? parsedData = null;
        try
        {
            parsedData = await _cvParserService.ParseCvAsync(fileBytes, file.FileName, file.ContentType);
        }
        catch (Exception ex)
        {
            return Ok(new UploadCvResponseDto { CvUrl = cvUrl, ParsedData = new CvParseResultDto() });
        }

        return Ok(new UploadCvResponseDto { CvUrl = cvUrl, ParsedData = parsedData });
    }

    [HttpPost("apply-cv")]
    public async Task<IActionResult> ApplyCv([FromBody] ApplyCvRequestDto request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(request.CvUrl) || request.ParsedData == null)
            return BadRequest("Invalid CV data");

        var updatedProfile = await _profileService.ApplyCvDataAsync(userId.Value, request.ParsedData, request.CvUrl);
        return Ok(updatedProfile);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
