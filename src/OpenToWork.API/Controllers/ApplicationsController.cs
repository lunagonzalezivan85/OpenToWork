using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IDeliveryService _deliveryService;
    private readonly AppDbContext _context;

    public ApplicationsController(IApplicationService applicationService, IDeliveryService deliveryService, AppDbContext context)
    {
        _applicationService = applicationService;
        _deliveryService = deliveryService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var candidateId = await GetCandidateIdAsync(userId.Value);
        if (candidateId == null) return BadRequest("Candidate profile not found");

        var vacancyExists = await _context.PT_Vacancies
            .AnyAsync(v => v.Id == dto.VacancyId && !v.IsDeleted);
        if (!vacancyExists) return NotFound("Vacancy not found");

        if (await _applicationService.HasAlreadyAppliedAsync(candidateId.Value, dto.VacancyId))
            return Conflict("You have already applied to this vacancy");

        var result = await _applicationService.ApplyAsync(candidateId.Value, dto);
        return CreatedAtAction(nameof(GetMyApplications), new { id = result.Id }, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var candidateId = await GetCandidateIdAsync(userId.Value);
        if (candidateId == null) return BadRequest("Candidate profile not found");

        var result = await _applicationService.GetApplicationsByCandidateAsync(candidateId.Value);
        return Ok(result);
    }

    [HttpGet("vacancy/{vacancyId}")]
    public async Task<IActionResult> GetByVacancy(Guid vacancyId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Embudo ciego: la empresa solo recibe conteos, nunca identidad de postulantes.
        var summary = await _deliveryService.GetVacancySummaryAsync(vacancyId, userId.Value);
        return summary == null ? NotFound() : Ok(summary);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task<Guid?> GetCandidateIdAsync(Guid userId)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        return candidate?.Id;
    }
}
