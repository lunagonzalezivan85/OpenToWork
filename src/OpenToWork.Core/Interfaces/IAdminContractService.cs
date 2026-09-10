using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminContractService
{
    Task<AdminVacancyContractDto?> GetByVacancyAsync(Guid vacancyId);
    Task<AdminVacancyContractDto?> SaveAsync(Guid vacancyId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress);
    Task<bool> SendAsync(Guid vacancyId, Guid adminId, string? ipAddress);
    Task<bool> DecideAsync(Guid vacancyId, bool accepted, string? reason, Guid adminId, string? ipAddress);
}
