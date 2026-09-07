using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/company-crm")]
public class CompanyCrmController : AdminControllerBase
{
    private readonly ICompanyCrmService _crmService;

    public CompanyCrmController(ICompanyCrmService crmService)
    {
        _crmService = crmService;
    }

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] string? search = null,
        [FromQuery] int? status = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _crmService.GetCompaniesAsync(search, status, assignedTo, page, pageSize);
        return Ok(result);
    }

    [HttpGet("companies/{id}")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        var result = await _crmService.GetCompanyAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
    {
        var result = await _crmService.CreateCompanyAsync(dto, AdminId, ClientIp);
        return result == null ? BadRequest() : Ok(result);
    }

    [HttpPut("companies/{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        var result = await _crmService.UpdateCompanyAsync(id, dto, AdminId, ClientIp);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("companies/{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var result = await _crmService.DeleteCompanyAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipeline(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? stage = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] string? search = null)
    {
        var result = await _crmService.GetPipelineAsync(page, pageSize, stage, assignedTo, search);
        return Ok(result);
    }

    [HttpGet("pipeline/{pipelineId}")]
    public async Task<IActionResult> GetPipelineDetail(Guid pipelineId)
    {
        var result = await _crmService.GetPipelineDetailAsync(pipelineId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignCompanyDto dto)
    {
        var result = await _crmService.AssignCompanyAsync(dto, AdminId, ClientIp);
        return Ok(result);
    }

    [HttpPut("pipeline/{pipelineId}/move-stage")]
    public async Task<IActionResult> MoveStage(Guid pipelineId, [FromBody] CompanyMoveStageDto dto)
    {
        var result = await _crmService.MoveStageAsync(pipelineId, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("pipeline/{pipelineId}/unassign")]
    public async Task<IActionResult> Unassign(Guid pipelineId)
    {
        var result = await _crmService.UnassignAsync(pipelineId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("pipeline/{pipelineId}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid pipelineId, [FromBody] DismissCompanyDto dto)
    {
        var result = await _crmService.DismissCompanyAsync(pipelineId, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("pipeline/{pipelineId}/restore")]
    public async Task<IActionResult> Restore(Guid pipelineId)
    {
        var result = await _crmService.RestoreCompanyAsync(pipelineId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("pipeline/{pipelineId}/reassign")]
    public async Task<IActionResult> Reassign(Guid pipelineId, [FromBody] ReassignCompanyDto dto)
    {
        var result = await _crmService.ReassignAsync(pipelineId, dto.NewUserId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var result = await _crmService.GetPlansAsync();
        return Ok(result);
    }
}

public class ReassignCompanyDto
{
    public Guid NewUserId { get; set; }
}
