using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminVacancyService
{
    Task<List<AdminVacancyDto>> GetVacanciesAsync(int page, int pageSize, int? status, Guid? companyId = null);
    Task<bool> ModerateAsync(Guid id, int status, Guid adminId, string? ipAddress);
    Task<AdminVacancyDto?> CreateAsync(AdminCreateVacancyDto dto, Guid adminId, string? ipAddress);
    Task<VacancyDto?> GetByIdAsync(Guid id);
    Task<bool> CreateApplicationAsync(Guid vacancyId, AdminCreateApplicationDto dto, Guid adminId, string? ipAddress);
}
