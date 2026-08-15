using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAlertService
{
    Task<List<AlertDto>> GetAlertsAsync(Guid userId);
}
