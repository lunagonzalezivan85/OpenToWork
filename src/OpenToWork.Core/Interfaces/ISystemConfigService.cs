using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface ISystemConfigService
{
    Task<List<SystemConfigDto>> GetAllAsync();
    Task UpdateBulkAsync(UpdateSystemConfigDto dto, Guid staffId);
    Task<CompanyIdentityDto> GetCompanyIdentityAsync();
}
