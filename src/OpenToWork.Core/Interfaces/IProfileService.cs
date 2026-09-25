using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IProfileService
{
    Task<CandidateProfileDto?> GetProfileAsync(Guid userId);
    /// <summary>Perfil de un candidato visto por otro usuario: el propio candidato lo ve completo; una
    /// empresa solo si Trato Directo le entrego a ese candidato (sin DNI, nacimiento ni direccion).
    /// Null para cualquier otro.</summary>
    Task<CandidateProfileDto?> GetCandidateByIdAsync(Guid candidateId, Guid viewerUserId);
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
