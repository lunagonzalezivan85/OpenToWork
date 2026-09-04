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

    Task<bool> RestoreCandidateAsync(Guid recruitmentId, Guid adminId, string? ipAddress);

    Task<bool> UnassignAsync(Guid recruitmentId, Guid adminId, string? ipAddress);

    Task<bool> UpdateCandidatePhoneAsync(Guid recruitmentId, string? phone, Guid adminId, string? ipAddress);

    Task<bool> UpdateChecklistNotesAsync(Guid checklistId, string? notes, Guid adminId, string? ipAddress);

    Task<TechnicalEvaluationDto?> AddTechnicalEvaluationAsync(Guid recruitmentId, AddTechnicalEvaluationDto dto, Guid adminId, string? ipAddress);
    Task<bool> UpdateTechnicalEvaluationAsync(Guid evaluationId, UpdateTechnicalEvaluationDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteTechnicalEvaluationAsync(Guid evaluationId, Guid adminId, string? ipAddress);

    Task<TechnicalEvaluationDto?> GetCulturalInterviewAsync(Guid recruitmentId);

    Task<CandidateRecruitmentPreferencesDto?> GetPreferencesAsync(Guid recruitmentId);
    Task<CandidateRecruitmentPreferencesDto?> SavePreferencesAsync(Guid recruitmentId, UpdateRecruitmentPreferencesDto dto, Guid adminId, string? ipAddress);

    Task<List<DocumentTypeDto>> GetDocumentTypesAsync();
    Task<List<RecruitmentDocumentDto>> GetDocumentsAsync(Guid recruitmentId);
    Task<RecruitmentDocumentDto?> RequestDocumentAsync(Guid recruitmentId, RequestDocumentDto dto, Guid adminId, string? ipAddress);
    Task<bool> UpdateDocumentStatusAsync(Guid documentId, UpdateDocumentStatusDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid adminId, string? ipAddress);

    Task<bool> UpdateMigrationInfoAsync(Guid recruitmentId, UpdateMigrationInfoDto dto, Guid adminId, string? ipAddress);

    Task<List<VacancyOptionDto>> GetVacancyOptionsAsync();
    Task<bool> LinkVacancyAsync(Guid recruitmentId, LinkVacancyDto dto, Guid adminId, string? ipAddress);
}
