using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface ICandidateService
{
    Task<CandidateDto?> GetCandidateByUserIdAsync(Guid userId);
    Task<CandidateDto?> GetCandidateByIdAsync(Guid id);
    Task<CandidateDto> CreateCandidateAsync(Guid userId, string createdBy);
    Task<CandidateDto> UpdateWizardStepAsync(Guid userId, UpdateCandidateWizardDto dto);
    Task<bool> IsWizardCompleteAsync(Guid userId);
    Task<CandidateProcessDto> GetMyProcessAsync(Guid userId);
}
