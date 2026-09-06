using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/skill-tests")]
[Authorize]
public class SkillTestsController : ControllerBase
{
    private readonly ISkillTestService _skillTestService;
    private readonly ICandidateService _candidateService;

    public SkillTestsController(ISkillTestService skillTestService, ICandidateService candidateService)
    {
        _skillTestService = skillTestService;
        _candidateService = candidateService;
    }

    /// <summary>Visible sin requerir perfil completo (fase-3-sub6.md pregunta 7).</summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] string? category)
    {
        var tests = await _skillTestService.GetAvailableTestsAsync(category);
        return Ok(tests);
    }

    /// <summary>Requiere WizardCompleted=true (pregunta 7). Idempotente si ya hay un intento en curso (pregunta 8).</summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var myCandidate = await GetMyCandidateAsync();
        if (myCandidate == null) return Forbid();

        try
        {
            var attempt = await _skillTestService.StartTestAsync(myCandidate.Id, id);
            return attempt == null ? NotFound() : Ok(attempt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("results/{id}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitTestAnswersDto dto, [FromQuery] int antiCheatFlags = 0)
    {
        var myCandidate = await GetMyCandidateAsync();
        if (myCandidate == null) return Forbid();

        var result = await _skillTestService.SubmitTestAsync(id, myCandidate.Id, dto, antiCheatFlags);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Historial propio - siempre del candidato autenticado, no hace falta {id} en la ruta.</summary>
    [HttpGet("results")]
    public async Task<IActionResult> GetMyResults()
    {
        var myCandidate = await GetMyCandidateAsync();
        if (myCandidate == null) return Forbid();

        var results = await _skillTestService.GetTestResultsAsync(myCandidate.Id);
        return Ok(results);
    }

    private async Task<CandidateDto?> GetMyCandidateAsync()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;
        return await _candidateService.GetCandidateByUserIdAsync(userId);
    }
}
