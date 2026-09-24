using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Interfaces;

public interface ICompanyCrmService
{
    Task<List<CompanyListDto>> GetCompaniesAsync(string? search = null, int? status = null, Guid? assignedTo = null, int page = 1, int pageSize = 50);
    Task<CompanyDetailDto?> GetCompanyAsync(Guid id);
    Task<CompanyDetailDto?> CreateCompanyAsync(CreateCompanyDto dto, Guid adminId, string? ipAddress);
    Task<CompanyDetailDto?> UpdateCompanyAsync(Guid id, UpdateCompanyDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteCompanyAsync(Guid id, Guid adminId, string? ipAddress);
    Task<bool> SetFeaturedAsync(Guid id, bool featured, Guid adminId, string? ipAddress);
    Task<bool> SetCompanyPlanTierAsync(Guid id, CompanyPlanTier tier, Guid adminId, string? ipAddress);

    Task<CompanyPipelineResultDto> GetPipelineAsync(int page, int pageSize, int? stage = null, Guid? assignedTo = null, string? search = null);
    Task<CompanyPipelineDetailDto?> GetPipelineDetailAsync(Guid pipelineId);
    Task<CompanyPipelineDto> AssignCompanyAsync(AssignCompanyDto dto, Guid adminId, string? ipAddress);
    Task<bool> MoveStageAsync(Guid pipelineId, CompanyMoveStageDto dto, Guid adminId, string? ipAddress);
    Task<bool> UnassignAsync(Guid pipelineId, Guid adminId, string? ipAddress);
    Task<bool> DismissCompanyAsync(Guid pipelineId, DismissCompanyDto dto, Guid adminId, string? ipAddress);
    Task<bool> RestoreCompanyAsync(Guid pipelineId, Guid adminId, string? ipAddress);
    Task<bool> ReassignAsync(Guid pipelineId, Guid newUserId, Guid adminId, string? ipAddress);
    Task<bool> UpdateNotesAsync(Guid pipelineId, UpdatePipelineNotesDto dto, Guid adminId, string? ipAddress);
    Task<List<PlanDto>> GetPlansAsync(PlanAudience audience = PlanAudience.Company);
    Task<List<PlanDto>> GetAllPlansAsync(PlanAudience? audience = null);
    Task<PlanDto> CreatePlanAsync(SavePlanDto dto, Guid adminId, string? ipAddress);
    Task<PlanDto?> UpdatePlanAsync(Guid id, SavePlanDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeletePlanAsync(Guid id, Guid adminId, string? ipAddress);
}
