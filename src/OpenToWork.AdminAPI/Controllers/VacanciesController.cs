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
    private readonly IAdminApplicationService _applicationService;

    public VacanciesController(IAdminVacancyService vacancyService, ICompatibilityService compatibilityService, IAdminApplicationService applicationService)
    {
        _vacancyService = vacancyService;
        _compatibilityService = compatibilityService;
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVacancies([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null, [FromQuery] Guid? companyId = null)
    {
        var vacancies = await _vacancyService.GetVacanciesAsync(page, pageSize, status, companyId);
        return Ok(vacancies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVacancy(Guid id)
    {
        var result = await _vacancyService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVacancy([FromBody] AdminCreateVacancyDto dto)
    {
        var result = await _vacancyService.CreateAsync(dto, AdminId, ClientIp);
        return result == null ? BadRequest() : Ok(result);
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

    /// <summary>Postulantes de la vacante (el admin si puede ver identidad; el embudo ciego solo aplica a empresas).</summary>
    [HttpGet("{id}/applicants")]
    public async Task<IActionResult> GetApplicants(Guid id)
    {
        var applicants = await _applicationService.GetByVacancyAsync(id);
        return Ok(applicants);
    }

    /// <summary>Candidatos que cumplen el perfil pero no se han postulado (prospectos para outreach).</summary>
    [HttpGet("{id}/non-applicant-matches")]
    public async Task<IActionResult> GetNonApplicantMatches(Guid id, [FromQuery] int? limit = null, [FromQuery] int minPercentage = 0)
    {
        var matches = await _compatibilityService.GetNonApplicantMatchesAsync(id, limit, minPercentage);
        return Ok(matches);
    }

    /// <summary>Postula al candidato seleccionado a nombre de TD (fuente AdminCurated).</summary>
    [HttpPost("{id}/applications")]
    public async Task<IActionResult> CreateApplication(Guid id, [FromBody] AdminCreateApplicationDto dto)
    {
        var created = await _vacancyService.CreateApplicationAsync(id, dto, AdminId, ClientIp);
        return created ? NoContent() : BadRequest();
    }
}
