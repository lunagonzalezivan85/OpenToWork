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
public class PermanentVacanciesController : ControllerBase
{
    private readonly IPermanentVacancyService _vacancyService;
    private readonly ICompatibilityService _compatibilityService;
    private readonly AppDbContext _context;

    public PermanentVacanciesController(IPermanentVacancyService vacancyService, ICompatibilityService compatibilityService, AppDbContext context)
    {
        _vacancyService = vacancyService;
        _compatibilityService = compatibilityService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVacancyDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var companyId = await GetCompanyIdAsync(userId.Value);
        if (companyId == null)
        {
            companyId = await GetOrCreateCompanyAsync(userId.Value);
        }

        var result = await _vacancyService.CreateVacancyAsync(companyId.Value, dto, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _vacancyService.GetVacancyByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetByCompany(Guid companyId)
    {
        var result = await _vacancyService.GetVacanciesByCompanyAsync(companyId);
        return Ok(result);
    }

    [HttpGet("my-company")]
    public async Task<IActionResult> GetMyCompanyVacancies()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var companyId = await GetCompanyIdAsync(userId.Value);
        if (companyId == null)
        {
            companyId = await GetOrCreateCompanyAsync(userId.Value);
        }

        var result = await _vacancyService.GetVacanciesByCompanyAsync(companyId.Value);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] SearchPermanentVacancyDto search)
    {
        var (items, total) = await _vacancyService.SearchVacanciesAsync(search);
        return Ok(new { items, total, page = search.Page, pageSize = search.PageSize });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVacancyDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _vacancyService.UpdateVacancyAsync(id, dto, userId.Value);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _vacancyService.DeleteVacancyAsync(id, userId.Value);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var published = await _vacancyService.PublishVacancyAsync(id, userId.Value);
        return published ? Ok() : NotFound();
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var closed = await _vacancyService.CloseVacancyAsync(id, userId.Value);
        return closed ? Ok() : NotFound();
    }

    [HttpPost("convert-temp/{tempVacancyId}")]
    public async Task<IActionResult> ConvertTemp(Guid tempVacancyId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var converted = await _vacancyService.ConvertTempVacancyAsync(tempVacancyId, userId.Value);
        return converted ? Ok() : NotFound();
    }

    // --- Fase 3, sub-fase 3.8: Portal de Empresa - Shortlist + Scorecard configurable ---
    // Solo lectura del shortlist (la empresa NO dispara el calculo - eso es admin/TD, ver
    // fase-3-sub4.md pregunta 6); el scorecard si es de la empresa (pregunta 4 de 3.8).

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(Guid id, [FromQuery] int? limit = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await OwnsVacancyAsync(id, userId.Value)) return Forbid();

        var shortlist = await _compatibilityService.GenerateShortlist(id, limit);
        return Ok(shortlist);
    }

    [HttpGet("{id}/scorecard")]
    public async Task<IActionResult> GetScorecard(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await OwnsVacancyAsync(id, userId.Value)) return Forbid();

        var vacancy = await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (vacancy == null) return NotFound();
        return Ok(new ScorecardDto { WeightsConfig = vacancy.WeightsConfig });
    }

    [HttpPut("{id}/scorecard")]
    public async Task<IActionResult> UpdateScorecard(Guid id, [FromBody] UpdateScorecardDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        if (!await OwnsVacancyAsync(id, userId.Value)) return Forbid();

        var vacancy = await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (vacancy == null) return NotFound();

        // null limpia la config custom y vuelve a los defaults de CompatibilityService.
        vacancy.WeightsConfig = dto.Skills.HasValue && dto.Experience.HasValue && dto.Location.HasValue
            ? System.Text.Json.JsonSerializer.Serialize(new { skills = dto.Skills, experience = dto.Experience, location = dto.Location })
            : null;
        vacancy.UpdatedAt = DateTime.UtcNow;
        vacancy.UpdatedBy = userId.Value;
        await _context.SaveChangesAsync();

        return Ok(new ScorecardDto { WeightsConfig = vacancy.WeightsConfig });
    }

    private async Task<bool> OwnsVacancyAsync(Guid vacancyId, Guid userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId == null) return false;
        return await _context.PT_Vacancies.AnyAsync(v => v.Id == vacancyId && v.PT_CompanyId == companyId && !v.IsDeleted);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task<Guid?> GetCompanyIdAsync(Guid userId)
    {
        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        return company?.Id;
    }

    private async Task<Guid> GetOrCreateCompanyAsync(Guid userId)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == userId);
        var company = new PTCompany
        {
            SCUserId = userId,
            Name = user?.Email?.Split('@')[0] ?? "Mi Empresa",
            ContactEmail = user?.Email,
            CreatedBy = userId
        };
        _context.PT_Companies.Add(company);
        await _context.SaveChangesAsync();
        return company.Id;
    }
}
