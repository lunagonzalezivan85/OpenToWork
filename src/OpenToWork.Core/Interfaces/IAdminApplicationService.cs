using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminApplicationService
{
    Task<List<AdminApplicationDto>> GetApplicationsAsync(int page, int pageSize, int? status);
    Task<List<AdminApplicationDto>> GetByVacancyAsync(Guid vacancyId);
}
