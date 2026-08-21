using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IRecruitmentService
{
    Task<RecruitmentPipelineResultDto> GetPipelineAsync(
        int page, int pageSize, int? stage = null, Guid? assignedTo = null, string? search = null);

    Task<RecruitmentDetailDto?> GetDetailAsync(Guid id);

    Task<RecruitmentDetailDto?> GetByUserIdAsync(Guid userId);

    Task<RecruitmentPipelineDto> AssignCandidateAsync(AssignCandidateDto dto, Guid adminId, string? ipAddress);

    Task<bool> MoveStageAsync(Guid recruitmentId, int toStage, string? notes, Guid adminId, string? ipAddress);

    Task<bool> ToggleInvestigationStepAsync(Guid recruitmentId, ToggleInvestigationStepDto dto, Guid adminId, string? ipAddress);

    Task<bool> StartInvestigationStepAsync(Guid recruitmentId, int step, Guid adminId, string? ipAddress);

    Task<ReferenceCheckDto?> AddReferenceAsync(Guid checklistId, AddReferenceDto dto, Guid adminId, string? ipAddress);

    Task<bool> UpdateReferenceStatusAsync(Guid referenceId, UpdateReferenceStatusDto dto, Guid adminId, string? ipAddress);

    Task<bool> DeleteReferenceAsync(Guid referenceId, Guid adminId, string? ipAddress);

    Task<InvestigationChecklistDto?> AddCustomValidationAsync(Guid recruitmentId, AddCustomValidationDto dto, Guid adminId, string? ipAddress);

    Task<bool> DeleteCustomValidationAsync(Guid checklistId, Guid adminId, string? ipAddress);

    Task<bool> DismissCandidateAsync(Guid recruitmentId, DismissCandidateDto dto, Guid adminId, string? ipAddress);

    Task<bool> UnassignAsync(Guid recruitmentId, Guid adminId, string? ipAddress);

    Task<bool> UpdateCandidatePhoneAsync(Guid recruitmentId, string? phone, Guid adminId, string? ipAddress);

    Task<bool> UpdateChecklistNotesAsync(Guid checklistId, string? notes, Guid adminId, string? ipAddress);
}
