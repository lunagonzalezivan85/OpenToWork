using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminContractService
{
    Task<AdminVacancyContractDto?> GetByIdAsync(Guid contractId);
    Task<AdminVacancyContractDto?> GetByCompanyAsync(Guid companyId);
    Task<AdminVacancyContractDto?> SaveAsync(Guid contractId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress);
    Task<AdminVacancyContractDto?> CreateAsync(Guid companyId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress);
    Task<bool> SendAsync(Guid contractId, Guid adminId, string? ipAddress);
    Task<bool> DecideAsync(Guid contractId, bool accepted, string? reason, Guid adminId, string? ipAddress);
}
