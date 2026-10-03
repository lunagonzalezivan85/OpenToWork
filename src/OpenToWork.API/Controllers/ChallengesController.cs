using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.API.Controllers;

/// <summary>
/// "Demuestra tus habilidades" (retos de hosteleria). Toda la propiedad de intentos y el acceso de
/// empresas se comprueban en IChallengeService; aqui solo se traduce el resultado a HTTP.
/// Errores: { error: "notFound" | "forbidden" | "locked" | "cooldown" | "attempts" | ... }.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class ChallengesController : ControllerBase
{
    private readonly IChallengeService _challenges;

    public ChallengesController(IChallengeService challenges)
    {
        _challenges = challenges;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult Map<T>(ChallengeOpResult<T> r) =>
        r.Error == null ? Ok(r.Value) : StatusCode(r.StatusCode, new { error = r.Error });

    [HttpGet("challenges/job-types")]
    public async Task<IActionResult> JobTypes() => Ok(await _challenges.GetJobTypesAsync());

    [HttpGet("challenges/job-types/{jobTypeId:guid}")]
    public async Task<IActionResult> Catalog(Guid jobTypeId) => Ok(await _challenges.GetCatalogAsync(UserId, jobTypeId));

    [HttpGet("challenges/{id:guid}")]
    public async Task<IActionResult> Intro(Guid id) => Map(await _challenges.GetIntroAsync(UserId, id));

    [HttpPost("challenges/{id:guid}/attempts")]
    public async Task<IActionResult> Start(Guid id, [FromBody] StartAttemptDto dto) =>
        Map(await _challenges.StartAttemptAsync(UserId, id, dto));

    [HttpGet("challenge-attempts/history")]
    public async Task<IActionResult> History() => Ok(await _challenges.GetHistoryAsync(UserId));

    [HttpGet("challenge-attempts/{id:guid}")]
    public async Task<IActionResult> Attempt(Guid id) => Map(await _challenges.GetAttemptAsync(UserId, id));

    [HttpPut("challenge-attempts/{id:guid}/answers/{activityKey}")]
    public async Task<IActionResult> SaveAnswer(Guid id, string activityKey, [FromBody] ChallengeResponse response) =>
        Map(await _challenges.SaveAnswerAsync(UserId, id, activityKey, response));

    [HttpPost("challenge-attempts/{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id) => Map(await _challenges.SubmitAsync(UserId, id));

    [HttpGet("challenge-attempts/{id:guid}/result")]
    public async Task<IActionResult> Result(Guid id) => Map(await _challenges.GetResultAsync(UserId, id));

    /// <summary>Empresa: resultados de un candidato que TD le ha entregado (sin respuestas).</summary>
    [HttpGet("company/candidates/{candidateId:guid}/challenge-results")]
    public async Task<IActionResult> CompanyResults(Guid candidateId) =>
        Map(await _challenges.GetResultsForCompanyAsync(UserId, candidateId));
}
