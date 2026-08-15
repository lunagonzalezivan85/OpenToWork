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
    private readonly AppDbContext _context;

    public PermanentVacanciesController(IPermanentVacancyService vacancyService, AppDbContext context)
    {
        _vacancyService = vacancyService;
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
