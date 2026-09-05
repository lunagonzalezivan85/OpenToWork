using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/vacancies")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class VacanciesController : AdminControllerBase
{
    private readonly IAdminVacancyService _vacancyService;
    private readonly ICompatibilityService _compatibilityService;

    public VacanciesController(IAdminVacancyService vacancyService, ICompatibilityService compatibilityService)
    {
        _vacancyService = vacancyService;
        _compatibilityService = compatibilityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVacancies([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null)
    {
        var vacancies = await _vacancyService.GetVacanciesAsync(page, pageSize, status);
        return Ok(vacancies);
    }

    [HttpPut("{id}/moderate")]
    public async Task<IActionResult> Moderate(Guid id, [FromBody] ModerateVacancyDto dto)
    {
        var result = await _vacancyService.ModerateAsync(id, dto.Status, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Dispara el calculo del Job Match Score contra todos los candidatos elegibles (perfil
    /// publico + wizard completo). Admin/TD-driven - no automatico (fase-3-sub4.md pregunta 6).
    /// </summary>
    [HttpPost("{id}/matches/calculate")]
    public async Task<IActionResult> CalculateMatches(Guid id)
    {
        var count = await _compatibilityService.CalculateMatchesForVacancyAsync(id);
        return Ok(new { candidatesEvaluated = count });
    }

    /// <summary>Shortlist rankeado por MatchPercentage descendente. limit por defecto 20.</summary>
    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(Guid id, [FromQuery] int? limit = null)
    {
        var shortlist = await _compatibilityService.GenerateShortlist(id, limit);
        return Ok(shortlist);
    }
}
