using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using System.Text;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/candidates")]
public class CandidatesController : AdminControllerBase
{
    private readonly IAdminCandidateService _candidateService;

    public CandidatesController(IAdminCandidateService candidateService)
    {
        _candidateService = candidateService;
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
        [FromQuery] bool sortDesc = true)
    {
        var result = await _candidateService.GetCandidatesAsync(
            page, pageSize, search, wizardCompleted, hasLinkedIn,
            hasPortfolio, hasCV, isActive, skillId, sortBy, sortDesc);
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
}

public class BulkActionDto
{
    public List<Guid> Ids { get; set; } = new();
}
