using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAdminCandidateService
{
    Task<CandidateConsoleResultDto> GetCandidatesAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? wizardCompleted = null,
        bool? hasLinkedIn = null,
        bool? hasPortfolio = null,
        bool? hasCV = null,
        bool? isActive = null,
        Guid? skillId = null,
        string? sortBy = null,
        bool sortDesc = true);

    Task<bool> BulkActivateAsync(List<Guid> ids, Guid adminId, string? ipAddress);
    Task<bool> BulkDeactivateAsync(List<Guid> ids, Guid adminId, string? ipAddress);
    Task<string> ExportCandidatesCsvAsync();
}
