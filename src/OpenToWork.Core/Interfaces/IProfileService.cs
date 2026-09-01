using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IProfileService
{
    Task<CandidateProfileDto?> GetProfileAsync(Guid userId);
    Task<CandidateProfileDto?> UpdateProfileAsync(Guid userId, UpdateCandidateProfileDto dto);
    Task<CandidateExperienceDto> AddExperienceAsync(Guid userId, CreateExperienceDto dto);
    Task<CandidateExperienceDto?> UpdateExperienceAsync(Guid experienceId, UpdateExperienceDto dto, Guid userId);
    Task<bool> DeleteExperienceAsync(Guid experienceId, Guid userId);
    Task<CandidateEducationDto> AddEducationAsync(Guid userId, CreateEducationDto dto);
    Task<CandidateEducationDto?> UpdateEducationAsync(Guid educationId, UpdateEducationDto dto, Guid userId);
    Task<bool> DeleteEducationAsync(Guid educationId, Guid userId);
    Task<CandidateCertificationDto> AddCertificationAsync(Guid userId, CreateCertificationDto dto);
    Task<CandidateCertificationDto?> UpdateCertificationAsync(Guid certificationId, UpdateCertificationDto dto, Guid userId);
    Task<bool> DeleteCertificationAsync(Guid certificationId, Guid userId);
    Task<CandidateProfileDto?> ApplyCvDataAsync(Guid userId, CvParseResultDto parsedData, string cvUrl);
}
