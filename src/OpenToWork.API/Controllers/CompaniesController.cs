using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.API.Controllers;

/// <summary>Endpoints públicos de empresas (sección "Empresas que confían en nosotros").</summary>
[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCompany()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var company = await _companyService.GetMyCompanyAsync(userId.Value);
        return company != null ? Ok(company) : NotFound();
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyCompany([FromBody] UpdateMyCompanyProfileDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var company = await _companyService.UpdateMyCompanyAsync(userId.Value, dto);
        return company != null ? Ok(company) : NotFound();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPublicCompanies([FromQuery] int? limit = null)
    {
        var companies = await _companyService.GetPublicCompaniesAsync(limit);
        return Ok(companies);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPublicCompany(Guid id)
    {
        var company = await _companyService.GetPublicCompanyAsync(id);
        return company != null ? Ok(company) : NotFound();
    }
}
