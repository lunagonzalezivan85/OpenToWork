using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin/recruitment")]
[RequireStaffRole(AdminStaffRole.Reclutador)]
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
        if (dto.ToStage > 0)
        {
            var prefs = await _recruitmentService.GetPreferencesAsync(id);
            if (prefs == null || !prefs.IsCompleted)
                return BadRequest(new { error = "Debe completar las preferencias del candidato antes de avanzar la etapa." });
        }

        if (dto.ToStage == 4)
        {
            var detail = await _recruitmentService.GetDetailAsync(id);
            if (detail != null && !detail.VacancyId.HasValue)
                return BadRequest(new { error = "Debe vincular el candidato a una vacante antes de marcarlo como Listo a Entregar." });
        }

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

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _recruitmentService.RestoreCandidateAsync(id, AdminId, ClientIp);
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

    [HttpPost("{id}/evaluations")]
    public async Task<IActionResult> AddEvaluation(Guid id, [FromBody] AddTechnicalEvaluationDto dto)
    {
        var result = await _recruitmentService.AddTechnicalEvaluationAsync(id, dto, AdminId, ClientIp);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPut("evaluations/{evaluationId}")]
    public async Task<IActionResult> UpdateEvaluation(Guid evaluationId, [FromBody] UpdateTechnicalEvaluationDto dto)
    {
        var result = await _recruitmentService.UpdateTechnicalEvaluationAsync(evaluationId, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("evaluations/{evaluationId}")]
    public async Task<IActionResult> DeleteEvaluation(Guid evaluationId)
    {
        var result = await _recruitmentService.DeleteTechnicalEvaluationAsync(evaluationId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("{id}/cultural-interview")]
    public async Task<IActionResult> GetCulturalInterview(Guid id)
    {
        var result = await _recruitmentService.GetCulturalInterviewAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("{id}/preferences")]
    public async Task<IActionResult> GetPreferences(Guid id)
    {
        var result = await _recruitmentService.GetPreferencesAsync(id);
        return result != null ? Ok(result) : Ok(new { isCompleted = false });
    }

    [HttpPut("{id}/preferences")]
    public async Task<IActionResult> SavePreferences(Guid id, [FromBody] UpdateRecruitmentPreferencesDto dto)
    {
        var result = await _recruitmentService.SavePreferencesAsync(id, dto, AdminId, ClientIp);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var result = await _recruitmentService.GetDocumentTypesAsync();
        return Ok(result);
    }

    [HttpGet("{id}/documents")]
    public async Task<IActionResult> GetDocuments(Guid id)
    {
        var result = await _recruitmentService.GetDocumentsAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/documents")]
    public async Task<IActionResult> RequestDocument(Guid id, [FromBody] RequestDocumentDto dto)
    {
        var result = await _recruitmentService.RequestDocumentAsync(id, dto, AdminId, ClientIp);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPut("documents/{documentId}")]
    public async Task<IActionResult> UpdateDocumentStatus(Guid documentId, [FromBody] UpdateDocumentStatusDto dto)
    {
        var result = await _recruitmentService.UpdateDocumentStatusAsync(documentId, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("documents/{documentId}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var result = await _recruitmentService.DeleteDocumentAsync(documentId, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/migration-info")]
    public async Task<IActionResult> UpdateMigrationInfo(Guid id, [FromBody] UpdateMigrationInfoDto dto)
    {
        var result = await _recruitmentService.UpdateMigrationInfoAsync(id, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("vacancies")]
    public async Task<IActionResult> GetVacancyOptions()
    {
        var result = await _recruitmentService.GetVacancyOptionsAsync();
        return Ok(result);
    }

    [HttpPut("{id}/link-vacancy")]
    public async Task<IActionResult> LinkVacancy(Guid id, [FromBody] LinkVacancyDto dto)
    {
        var result = await _recruitmentService.LinkVacancyAsync(id, dto, AdminId, ClientIp);
        return result ? NoContent() : NotFound();
    }
}
