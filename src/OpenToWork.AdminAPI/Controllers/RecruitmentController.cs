using Microsoft.AspNetCore.Mvc;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/recruitment")]
public class RecruitmentController : AdminControllerBase
{
    private readonly IRecruitmentService _recruitmentService;

    public RecruitmentController(IRecruitmentService recruitmentService)
    {
        _recruitmentService = recruitmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPipeline(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? stage = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] string? search = null)
    {
        var result = await _recruitmentService.GetPipelineAsync(page, pageSize, stage, assignedTo, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var detail = await _recruitmentService.GetDetailAsync(id);
        return detail == null ? NotFound() : Ok(detail);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignCandidateDto dto)
    {
        var result = await _recruitmentService.AssignCandidateAsync(dto, AdminId, ClientIp);
        return Ok(result);
    }

    [HttpPut("{id}/move-stage")]
    public async Task<IActionResult> MoveStage(Guid id, [FromBody] MoveStageDto dto)
    {
        var result = await _recruitmentService.MoveStageAsync(id, dto.ToStage, dto.Notes, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/investigation")]
    public async Task<IActionResult> ToggleInvestigationStep(Guid id, [FromBody] ToggleInvestigationStepDto dto)
    {
        var result = await _recruitmentService.ToggleInvestigationStepAsync(id, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id, [FromBody] DismissCandidateDto dto)
    {
        var result = await _recruitmentService.DismissCandidateAsync(id, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/unassign")]
    public async Task<IActionResult> Unassign(Guid id)
    {
        var result = await _recruitmentService.UnassignAsync(id, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }
}
