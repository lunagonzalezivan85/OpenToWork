using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardMetricsDto> GetMetricsAsync();
    Task<BusinessMetricsDto> GetBusinessMetricsAsync();
}
