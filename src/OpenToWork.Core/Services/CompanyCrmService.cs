using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class CompanyCrmService : ICompanyCrmService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;

    public CompanyCrmService(AppDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public async Task<List<CompanyListDto>> GetCompaniesAsync(string? search = null, int? status = null, Guid? assignedTo = null, int page = 1, int pageSize = 50)
    {
        var query = _db.PT_Companies
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.LegalName != null && c.LegalName.ToLower().Contains(term)) ||
                (c.ContactEmail != null && c.ContactEmail.ToLower().Contains(term)) ||
                (c.Industry != null && c.Industry.ToLower().Contains(term)));
        }

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (assignedTo.HasValue)
        {
            query = query.Where(c => c.Pipelines.Any(p => p.AssignedToUserId == assignedTo.Value && !p.IsDeleted && !p.IsDismissed));
        }

        var companies = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListDto
            {
                Id = c.Id,
                Name = c.Name,
                LegalName = c.LegalName,
                TaxId = c.TaxId,
                Industry = c.Industry,
                Website = c.Website,
                Country = c.Country,
                City = c.City,
                ContactName = c.ContactName,
                ContactEmail = c.ContactEmail,
                ContactPhone = c.ContactPhone,
                Status = c.Status,
                IsVerified = c.IsVerified,
                CreatedAt = c.CreatedAt,
                VacancyCount = c.Vacancies.Count(v => !v.IsDeleted),
                CurrentStage = c.Pipelines.Where(p => !p.IsDeleted && !p.IsDismissed).Select(p => (int?)p.CurrentStage).FirstOrDefault(),
                AssignedToUserId = c.Pipelines.Where(p => !p.IsDeleted && !p.IsDismissed).Select(p => p.AssignedToUserId).FirstOrDefault(),
                AssignedToName = c.Pipelines.Where(p => !p.IsDeleted && !p.IsDismissed).Select(p => p.AssignedToUser != null ? p.AssignedToUser.Email : null).FirstOrDefault()
            })
            .ToListAsync();

        return companies;
    }

    public async Task<CompanyDetailDto?> GetCompanyAsync(Guid id)
    {
        var company = await _db.PT_Companies
            .Include(c => c.Pipelines.Where(p => !p.IsDeleted && !p.IsDismissed))
                .ThenInclude(p => p.AssignedToUser)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (company == null) return null;

        var pipeline = company.Pipelines.FirstOrDefault();
        return new CompanyDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            LegalName = company.LegalName,
            TaxId = company.TaxId,
            Industry = company.Industry,
            Website = company.Website,
            LogoUrl = company.LogoUrl,
            Description = company.Description,
            Country = company.Country,
            City = company.City,
            Address = company.Address,
            CompanySize = company.CompanySize,
            ContactName = company.ContactName,
            ContactEmail = company.ContactEmail,
            ContactPhone = company.ContactPhone,
            ContactPosition = company.ContactPosition,
            LinkedInUrl = company.LinkedInUrl,
            Status = company.Status,
            IsVerified = company.IsVerified,
            CreatedAt = company.CreatedAt,
            SCUserId = company.SCUserId,
            Pipeline = pipeline != null ? new CompanyPipelineDto
            {
                Id = pipeline.Id,
                CompanyId = company.Id,
                CompanyName = company.Name,
                Industry = company.Industry,
                ContactName = company.ContactName,
                ContactEmail = company.ContactEmail,
                ContactPhone = company.ContactPhone,
                CurrentStage = pipeline.CurrentStage,
                AssignedToName = pipeline.AssignedToUser?.Email,
                AssignedToUserId = pipeline.AssignedToUserId,
                AssignedAt = pipeline.AssignedAt,
                StageEnteredAt = pipeline.StageEnteredAt,
                CreatedAt = pipeline.CreatedAt,
                Notes = pipeline.Notes,
                IsDismissed = pipeline.IsDismissed,
                DismissalReason = pipeline.DismissalReason
            } : null
        };
    }

    public async Task<CompanyDetailDto?> CreateCompanyAsync(CreateCompanyDto dto, Guid adminId, string? ipAddress)
    {
        var company = new PTCompany
        {
            Name = dto.Name,
            LegalName = dto.LegalName,
            TaxId = dto.TaxId,
            Industry = dto.Industry,
            Website = dto.Website,
            Description = dto.Description,
            Country = dto.Country,
            City = dto.City,
            Address = dto.Address,
            CompanySize = dto.CompanySize,
            ContactName = dto.ContactName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            ContactPosition = dto.ContactPosition,
            LinkedInUrl = dto.LinkedInUrl,
            Status = (int)CompanyStatus.Prospecto,
            SCUserId = adminId,
            CreatedBy = adminId
        };

        _db.PT_Companies.Add(company);

        var pipeline = new PTCompanyPipeline
        {
            PT_CompanyId = company.Id,
            CurrentStage = (int)CompanyPipelineStage.Lead,
            AssignedToUserId = adminId,
            AssignedByUserId = adminId,
            AssignedAt = DateTime.UtcNow,
            StageEnteredAt = DateTime.UtcNow,
            CreatedBy = adminId
        };
        _db.PT_CompanyPipelines.Add(pipeline);

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Create", "PTCompany", company.Id, $"Empresa creada: {company.Name}", ipAddress);

        return await GetCompanyAsync(company.Id);
    }

    public async Task<CompanyDetailDto?> UpdateCompanyAsync(Guid id, UpdateCompanyDto dto, Guid adminId, string? ipAddress)
    {
        var company = await _db.PT_Companies.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (company == null) return null;

        company.Name = dto.Name;
        company.LegalName = dto.LegalName;
        company.TaxId = dto.TaxId;
        company.Industry = dto.Industry;
        company.Website = dto.Website;
        company.Description = dto.Description;
        company.Country = dto.Country;
        company.City = dto.City;
        company.Address = dto.Address;
        company.CompanySize = dto.CompanySize;
        company.ContactName = dto.ContactName;
        company.ContactEmail = dto.ContactEmail;
        company.ContactPhone = dto.ContactPhone;
        company.ContactPosition = dto.ContactPosition;
        company.LinkedInUrl = dto.LinkedInUrl;
        company.Status = dto.Status;
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Update", "PTCompany", id, $"Empresa actualizada: {company.Name}", ipAddress);

        return await GetCompanyAsync(id);
    }

    public async Task<bool> DeleteCompanyAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var company = await _db.PT_Companies.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (company == null) return false;

        company.IsDeleted = true;
        company.DeletedAt = DateTime.UtcNow;
        company.DeletedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Delete", "PTCompany", id, $"Empresa eliminada: {company.Name}", ipAddress);

        return true;
    }

    public async Task<CompanyPipelineResultDto> GetPipelineAsync(int page, int pageSize, int? stage = null, Guid? assignedTo = null, string? search = null)
    {
        var query = _db.PT_CompanyPipelines
            .Include(p => p.Company)
            .Include(p => p.AssignedToUser)
            .Where(p => !p.IsDeleted);

        if (stage.HasValue)
            query = query.Where(p => p.CurrentStage == stage.Value);

        if (assignedTo.HasValue)
            query = query.Where(p => p.AssignedToUserId == assignedTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(p =>
                p.Company.Name.ToLower().Contains(term) ||
                (p.Company.ContactEmail != null && p.Company.ContactEmail.ToLower().Contains(term)) ||
                (p.Company.Industry != null && p.Company.Industry.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.StageEnteredAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new CompanyPipelineDto
            {
                Id = p.Id,
                CompanyId = p.PT_CompanyId,
                CompanyName = p.Company.Name,
                Industry = p.Company.Industry,
                ContactName = p.Company.ContactName,
                ContactEmail = p.Company.ContactEmail,
                ContactPhone = p.Company.ContactPhone,
                CurrentStage = p.CurrentStage,
                AssignedToName = p.AssignedToUser != null ? p.AssignedToUser.Email : null,
                AssignedToUserId = p.AssignedToUserId,
                AssignedAt = p.AssignedAt,
                StageEnteredAt = p.StageEnteredAt,
                CreatedAt = p.CreatedAt,
                Notes = p.Notes,
                IsDismissed = p.IsDismissed,
                DismissalReason = p.DismissalReason
            })
            .ToListAsync();

        var countByStage = await _db.PT_CompanyPipelines
            .Where(p => !p.IsDeleted && !p.IsDismissed)
            .GroupBy(p => p.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Stage, x => x.Count);

        return new CompanyPipelineResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            CountByStage = countByStage
        };
    }

    public async Task<CompanyPipelineDetailDto?> GetPipelineDetailAsync(Guid pipelineId)
    {
        var pipeline = await _db.PT_CompanyPipelines
            .Include(p => p.Company)
            .Include(p => p.AssignedToUser)
            .Include(p => p.StageLogs)
                .ThenInclude(s => s.ChangedByUser)
            .Include(p => p.DismissedByUser)
            .FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);

        if (pipeline == null) return null;

        return new CompanyPipelineDetailDto
        {
            Id = pipeline.Id,
            CompanyId = pipeline.PT_CompanyId,
            CompanyName = pipeline.Company.Name,
            LegalName = pipeline.Company.LegalName,
            TaxId = pipeline.Company.TaxId,
            Industry = pipeline.Company.Industry,
            Website = pipeline.Company.Website,
            Description = pipeline.Company.Description,
            Country = pipeline.Company.Country,
            City = pipeline.Company.City,
            Address = pipeline.Company.Address,
            CompanySize = pipeline.Company.CompanySize,
            ContactName = pipeline.Company.ContactName,
            ContactEmail = pipeline.Company.ContactEmail,
            ContactPhone = pipeline.Company.ContactPhone,
            ContactPosition = pipeline.Company.ContactPosition,
            LinkedInUrl = pipeline.Company.LinkedInUrl,
            Status = pipeline.Company.Status,
            CurrentStage = pipeline.CurrentStage,
            AssignedToName = pipeline.AssignedToUser?.Email,
            AssignedToUserId = pipeline.AssignedToUserId,
            AssignedAt = pipeline.AssignedAt,
            StageEnteredAt = pipeline.StageEnteredAt,
            CreatedAt = pipeline.CreatedAt,
            Notes = pipeline.Notes,
            IsDismissed = pipeline.IsDismissed,
            DismissalReason = pipeline.DismissalReason,
            DismissedAt = pipeline.DismissedAt,
            DismissedByName = pipeline.DismissedByUser?.Email,
            StageLogs = pipeline.StageLogs.OrderBy(s => s.CreatedAt).Select(s => new CompanyStageLogDto
            {
                FromStage = s.FromStage,
                ToStage = s.ToStage,
                ChangedByName = s.ChangedByUser.Email,
                CreatedAt = s.CreatedAt,
                Notes = s.Notes
            }).ToList()
        };
    }

    public async Task<CompanyPipelineDto> AssignCompanyAsync(AssignCompanyDto dto, Guid adminId, string? ipAddress)
    {
        var existingPipeline = await _db.PT_CompanyPipelines
            .FirstOrDefaultAsync(p => p.PT_CompanyId == dto.CompanyId && !p.IsDeleted && !p.IsDismissed);

        if (existingPipeline != null)
        {
            existingPipeline.AssignedToUserId = dto.AssignedToUserId;
            existingPipeline.AssignedByUserId = adminId;
            existingPipeline.AssignedAt = DateTime.UtcNow;
            existingPipeline.Notes = dto.Notes ?? existingPipeline.Notes;
            existingPipeline.UpdatedAt = DateTime.UtcNow;
            existingPipeline.UpdatedBy = adminId;

            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(adminId, "CompanyCrm.Assign", "PTCompanyPipeline", existingPipeline.Id, "Empresa asignada a usuario", ipAddress);

            return new CompanyPipelineDto
            {
                Id = existingPipeline.Id,
                CompanyId = existingPipeline.PT_CompanyId,
                CurrentStage = existingPipeline.CurrentStage,
                AssignedToUserId = existingPipeline.AssignedToUserId,
                AssignedAt = existingPipeline.AssignedAt,
                CreatedAt = existingPipeline.CreatedAt,
                Notes = existingPipeline.Notes,
                IsDismissed = existingPipeline.IsDismissed
            };
        }

        var pipeline = new PTCompanyPipeline
        {
            PT_CompanyId = dto.CompanyId,
            CurrentStage = (int)CompanyPipelineStage.Lead,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedByUserId = adminId,
            AssignedAt = DateTime.UtcNow,
            StageEnteredAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedBy = adminId
        };

        _db.PT_CompanyPipelines.Add(pipeline);
        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Assign", "PTCompanyPipeline", pipeline.Id, "Empresa asignada a pipeline", ipAddress);

        return new CompanyPipelineDto
        {
            Id = pipeline.Id,
            CompanyId = pipeline.PT_CompanyId,
            CurrentStage = pipeline.CurrentStage,
            AssignedToUserId = pipeline.AssignedToUserId,
            AssignedAt = pipeline.AssignedAt,
            CreatedAt = pipeline.CreatedAt,
            Notes = pipeline.Notes,
            IsDismissed = pipeline.IsDismissed
        };
    }

    public async Task<bool> MoveStageAsync(Guid pipelineId, CompanyMoveStageDto dto, Guid adminId, string? ipAddress)
    {
        var pipeline = await _db.PT_CompanyPipelines.FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);
        if (pipeline == null) return false;

        var fromStage = pipeline.CurrentStage;
        pipeline.CurrentStage = dto.ToStage;
        pipeline.StageEnteredAt = DateTime.UtcNow;
        pipeline.Notes = dto.Notes ?? pipeline.Notes;
        pipeline.UpdatedAt = DateTime.UtcNow;
        pipeline.UpdatedBy = adminId;

        var log = new PTCompanyStageLog
        {
            PT_CompanyPipelineId = pipelineId,
            FromStage = fromStage,
            ToStage = dto.ToStage,
            ChangedByUserId = adminId,
            Notes = dto.Notes,
            CreatedBy = adminId
        };
        _db.PT_CompanyStageLogs.Add(log);

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.MoveStage", "PTCompanyPipeline", pipelineId, $"Empresa movida de etapa {fromStage} a {dto.ToStage}", ipAddress);

        return true;
    }

    public async Task<bool> UnassignAsync(Guid pipelineId, Guid adminId, string? ipAddress)
    {
        var pipeline = await _db.PT_CompanyPipelines.FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);
        if (pipeline == null) return false;

        pipeline.AssignedToUserId = null;
        pipeline.AssignedByUserId = null;
        pipeline.AssignedAt = null;
        pipeline.UpdatedAt = DateTime.UtcNow;
        pipeline.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Unassign", "PTCompanyPipeline", pipelineId, "Empresa desasignada", ipAddress);

        return true;
    }

    public async Task<bool> DismissCompanyAsync(Guid pipelineId, DismissCompanyDto dto, Guid adminId, string? ipAddress)
    {
        var pipeline = await _db.PT_CompanyPipelines.FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);
        if (pipeline == null) return false;

        pipeline.IsDismissed = true;
        pipeline.DismissalReason = dto.Reason;
        pipeline.DismissedAt = DateTime.UtcNow;
        pipeline.DismissedByUserId = adminId;
        pipeline.UpdatedAt = DateTime.UtcNow;
        pipeline.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Dismiss", "PTCompanyPipeline", pipelineId, $"Empresa descartada: {dto.Reason}", ipAddress);

        return true;
    }

    public async Task<bool> RestoreCompanyAsync(Guid pipelineId, Guid adminId, string? ipAddress)
    {
        var pipeline = await _db.PT_CompanyPipelines.FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);
        if (pipeline == null) return false;

        pipeline.IsDismissed = false;
        pipeline.DismissalReason = null;
        pipeline.DismissedAt = null;
        pipeline.DismissedByUserId = null;
        pipeline.UpdatedAt = DateTime.UtcNow;
        pipeline.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Restore", "PTCompanyPipeline", pipelineId, "Empresa restaurada", ipAddress);

        return true;
    }

    public async Task<bool> ReassignAsync(Guid pipelineId, Guid newUserId, Guid adminId, string? ipAddress)
    {
        var pipeline = await _db.PT_CompanyPipelines.FirstOrDefaultAsync(p => p.Id == pipelineId && !p.IsDeleted);
        if (pipeline == null) return false;

        pipeline.AssignedToUserId = newUserId;
        pipeline.AssignedByUserId = adminId;
        pipeline.AssignedAt = DateTime.UtcNow;
        pipeline.UpdatedAt = DateTime.UtcNow;
        pipeline.UpdatedBy = adminId;

        await _db.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CompanyCrm.Reassign", "PTCompanyPipeline", pipelineId, "Empresa reasignada a otro usuario", ipAddress);

        return true;
    }

    public async Task<List<PlanDto>> GetPlansAsync()
    {
        return await _db.PT_Plans
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Currency = p.Currency,
                SortOrder = p.SortOrder
            })
            .ToListAsync();
    }
}
