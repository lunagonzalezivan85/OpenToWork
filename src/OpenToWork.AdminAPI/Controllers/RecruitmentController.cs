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

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var detail = await _recruitmentService.GetByUserIdAsync(userId);
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

    [HttpPut("{id}/investigation/{step}/start")]
    public async Task<IActionResult> StartInvestigationStep(Guid id, int step)
    {
        var result = await _recruitmentService.StartInvestigationStepAsync(id, step, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("investigation/{checklistId}/references")]
    public async Task<IActionResult> AddReference(Guid checklistId, [FromBody] AddReferenceDto dto)
    {
        var result = await _recruitmentService.AddReferenceAsync(checklistId, dto, AdminId, ClientIp);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("references/{referenceId}")]
    public async Task<IActionResult> UpdateReferenceStatus(Guid referenceId, [FromBody] UpdateReferenceStatusDto dto)
    {
        var result = await _recruitmentService.UpdateReferenceStatusAsync(referenceId, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("references/{referenceId}")]
    public async Task<IActionResult> DeleteReference(Guid referenceId)
    {
        var result = await _recruitmentService.DeleteReferenceAsync(referenceId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/investigation/custom")]
    public async Task<IActionResult> AddCustomValidation(Guid id, [FromBody] AddCustomValidationDto dto)
    {
        var result = await _recruitmentService.AddCustomValidationAsync(id, dto, AdminId, ClientIp);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("investigation/{checklistId}")]
    public async Task<IActionResult> DeleteCustomValidation(Guid checklistId)
    {
        var result = await _recruitmentService.DeleteCustomValidationAsync(checklistId, AdminId, ClientIp);
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

    [HttpPut("{id}/candidate-phone")]
    public async Task<IActionResult> UpdateCandidatePhone(Guid id, [FromBody] UpdateCandidatePhoneDto dto)
    {
        var result = await _recruitmentService.UpdateCandidatePhoneAsync(id, dto.Phone, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("investigation/{checklistId}/notes")]
    public async Task<IActionResult> UpdateChecklistNotes(Guid checklistId, [FromBody] UpdateChecklistNotesDto dto)
    {
        var result = await _recruitmentService.UpdateChecklistNotesAsync(checklistId, dto.Notes, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }
}
