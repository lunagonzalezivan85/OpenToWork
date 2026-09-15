using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>Catalogo de niveles/tipos de puesto y lista de precios B2B para contratos de vacantes.</summary>
[Route("api/admin/pricing")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class PricingController : AdminControllerBase
{
    private readonly IJobPricingService _pricing;

    public PricingController(IJobPricingService pricing)
    {
        _pricing = pricing;
    }

    [HttpGet("job-levels")]
    public async Task<IActionResult> GetJobLevels() => Ok(await _pricing.GetJobLevelsAsync());

    [HttpPost("job-levels")]
    public async Task<IActionResult> CreateJobLevel([FromBody] SaveJobLevelDto dto)
    {
        try
        {
            return Ok(await _pricing.CreateJobLevelAsync(dto, AdminId, ClientIp));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("job-levels/{id:guid}")]
    public async Task<IActionResult> UpdateJobLevel(Guid id, [FromBody] SaveJobLevelDto dto)
    {
        try
        {
            var result = await _pricing.UpdateJobLevelAsync(id, dto, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("job-levels/{id:guid}")]
    public async Task<IActionResult> DeleteJobLevel(Guid id)
    {
        try
        {
            var result = await _pricing.DeleteJobLevelAsync(id, AdminId, ClientIp);
            return result ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("job-types")]
    public async Task<IActionResult> GetJobTypes() => Ok(await _pricing.GetJobTypesAsync());

    [HttpPost("job-types")]
    public async Task<IActionResult> CreateJobType([FromBody] SaveJobTypeDto dto)
    {
        try
        {
            return Ok(await _pricing.CreateJobTypeAsync(dto, AdminId, ClientIp));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("job-types/{id:guid}")]
    public async Task<IActionResult> UpdateJobType(Guid id, [FromBody] SaveJobTypeDto dto)
    {
        try
        {
            var result = await _pricing.UpdateJobTypeAsync(id, dto, AdminId, ClientIp);
            return result == null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("job-types/{id:guid}")]
    public async Task<IActionResult> DeleteJobType(Guid id)
    {
        var result = await _pricing.DeleteJobTypeAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("job-types/{id:guid}/price-history")]
    public async Task<IActionResult> GetPriceHistory(Guid id) => Ok(await _pricing.GetPriceHistoryAsync(id));

    [HttpPost("job-types/{id:guid}/price")]
    public async Task<IActionResult> SetPrice(Guid id, [FromBody] SetJobTypePriceDto dto)
    {
        try
        {
            return Ok(await _pricing.SetPriceAsync(id, dto, AdminId, ClientIp));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("job-types/{id:guid}/skills")]
    public async Task<IActionResult> GetJobTypeSkills(Guid id)
    {
        try
        {
            return Ok(await _pricing.GetJobTypeSkillsAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("job-types/{id:guid}/skills")]
    public async Task<IActionResult> SetJobTypeSkills(Guid id, [FromBody] SetJobTypeSkillsDto dto)
    {
        try
        {
            return Ok(await _pricing.SetJobTypeSkillsAsync(id, dto.SkillIds, AdminId, ClientIp));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
