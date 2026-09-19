using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class JobPricingService : IJobPricingService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public JobPricingService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    // ===== Niveles de puesto =====

    public async Task<List<JobLevelDto>> GetJobLevelsAsync()
    {
        return await _context.PT_JobLevels
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .Select(l => new JobLevelDto
            {
                Id = l.Id,
                Name = l.Name,
                ReferenceCoverageDays = l.ReferenceCoverageDays,
                WarrantyDays = l.WarrantyDays,
                SortOrder = l.SortOrder,
                IsActive = l.IsActive,
                JobTypeCount = l.JobTypes.Count(t => !t.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<JobLevelDto> CreateJobLevelAsync(SaveJobLevelDto dto, Guid adminId, string? ipAddress)
    {
        var level = new PTJobLevel
        {
            Name = dto.Name.Trim(),
            ReferenceCoverageDays = dto.ReferenceCoverageDays,
            WarrantyDays = dto.WarrantyDays,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedBy = adminId
        };
        _context.PT_JobLevels.Add(level);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CreateJobLevel", "PT_JobLevels", level.Id, $"{{\"name\":\"{level.Name}\"}}", ipAddress);

        return new JobLevelDto
        {
            Id = level.Id,
            Name = level.Name,
            ReferenceCoverageDays = level.ReferenceCoverageDays,
            WarrantyDays = level.WarrantyDays,
            SortOrder = level.SortOrder,
            IsActive = level.IsActive,
            JobTypeCount = 0
        };
    }

    public async Task<JobLevelDto?> UpdateJobLevelAsync(Guid id, SaveJobLevelDto dto, Guid adminId, string? ipAddress)
    {
        var level = await _context.PT_JobLevels.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (level == null) return null;

        level.Name = dto.Name.Trim();
        level.ReferenceCoverageDays = dto.ReferenceCoverageDays;
        level.WarrantyDays = dto.WarrantyDays;
        level.SortOrder = dto.SortOrder;
        level.IsActive = dto.IsActive;
        level.UpdatedAt = DateTime.UtcNow;
        level.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateJobLevel", "PT_JobLevels", level.Id, $"{{\"name\":\"{level.Name}\"}}", ipAddress);

        var jobTypeCount = await _context.PT_JobTypes.CountAsync(t => t.PT_JobLevelId == id && !t.IsDeleted);
        return new JobLevelDto
        {
            Id = level.Id,
            Name = level.Name,
            ReferenceCoverageDays = level.ReferenceCoverageDays,
            WarrantyDays = level.WarrantyDays,
            SortOrder = level.SortOrder,
            IsActive = level.IsActive,
            JobTypeCount = jobTypeCount
        };
    }

    public async Task<bool> DeleteJobLevelAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var level = await _context.PT_JobLevels.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        if (level == null) return false;

        var hasJobTypes = await _context.PT_JobTypes.AnyAsync(t => t.PT_JobLevelId == id && !t.IsDeleted);
        if (hasJobTypes)
            throw new InvalidOperationException("No se puede eliminar un nivel de puesto que tiene tipos de puesto asignados. Reasigna o elimina esos tipos primero.");

        level.IsDeleted = true;
        level.DeletedAt = DateTime.UtcNow;
        level.DeletedBy = adminId;
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteJobLevel", "PT_JobLevels", level.Id, null, ipAddress);
        return true;
    }

    // ===== Tipos de puesto =====

    public async Task<List<JobTypeDto>> GetJobTypesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.PT_JobTypes
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Select(t => new JobTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                PT_JobLevelId = t.PT_JobLevelId,
                JobLevelName = t.JobLevel.Name,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive,
                CurrentPrice = t.Prices
                    .Where(p => !p.IsDeleted && p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now))
                    .Select(p => (decimal?)p.BasePrice)
                    .FirstOrDefault(),
                Currency = t.Prices
                    .Where(p => !p.IsDeleted && p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now))
                    .Select(p => p.Currency)
                    .FirstOrDefault() ?? "EUR",
                PriceEffectiveFrom = t.Prices
                    .Where(p => !p.IsDeleted && p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now))
                    .Select(p => (DateTime?)p.EffectiveFrom)
                    .FirstOrDefault(),
                LevelWarrantyDays = t.JobLevel.WarrantyDays
            })
            .ToListAsync();
    }

    public async Task<JobTypeDto> CreateJobTypeAsync(SaveJobTypeDto dto, Guid adminId, string? ipAddress)
    {
        var level = await _context.PT_JobLevels.FirstOrDefaultAsync(l => l.Id == dto.PT_JobLevelId && !l.IsDeleted)
            ?? throw new InvalidOperationException("El nivel de puesto seleccionado no existe.");

        var jobType = new PTJobType
        {
            Name = dto.Name.Trim(),
            PT_JobLevelId = dto.PT_JobLevelId,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedBy = adminId
        };
        _context.PT_JobTypes.Add(jobType);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CreateJobType", "PT_JobTypes", jobType.Id, $"{{\"name\":\"{jobType.Name}\"}}", ipAddress);

        return new JobTypeDto
        {
            Id = jobType.Id,
            Name = jobType.Name,
            PT_JobLevelId = jobType.PT_JobLevelId,
            JobLevelName = level.Name,
            SortOrder = jobType.SortOrder,
            IsActive = jobType.IsActive
        };
    }

    public async Task<JobTypeDto?> UpdateJobTypeAsync(Guid id, SaveJobTypeDto dto, Guid adminId, string? ipAddress)
    {
        var jobType = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (jobType == null) return null;

        var level = await _context.PT_JobLevels.FirstOrDefaultAsync(l => l.Id == dto.PT_JobLevelId && !l.IsDeleted)
            ?? throw new InvalidOperationException("El nivel de puesto seleccionado no existe.");

        jobType.Name = dto.Name.Trim();
        jobType.PT_JobLevelId = dto.PT_JobLevelId;
        jobType.SortOrder = dto.SortOrder;
        jobType.IsActive = dto.IsActive;
        jobType.UpdatedAt = DateTime.UtcNow;
        jobType.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateJobType", "PT_JobTypes", jobType.Id, $"{{\"name\":\"{jobType.Name}\"}}", ipAddress);

        var price = await GetActivePriceAsync(id);
        return new JobTypeDto
        {
            Id = jobType.Id,
            Name = jobType.Name,
            PT_JobLevelId = jobType.PT_JobLevelId,
            JobLevelName = level.Name,
            SortOrder = jobType.SortOrder,
            IsActive = jobType.IsActive,
            CurrentPrice = price?.BasePrice,
            Currency = price?.Currency ?? "EUR",
            PriceEffectiveFrom = price?.EffectiveFrom
        };
    }

    public async Task<bool> DeleteJobTypeAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var jobType = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (jobType == null) return false;

        jobType.IsDeleted = true;
        jobType.DeletedAt = DateTime.UtcNow;
        jobType.DeletedBy = adminId;
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteJobType", "PT_JobTypes", jobType.Id, null, ipAddress);
        return true;
    }

    // ===== Lista de precios =====

    public async Task<List<JobTypePriceDto>> GetPriceHistoryAsync(Guid jobTypeId)
    {
        return await _context.PT_JobTypePrices
            .Where(p => p.PT_JobTypeId == jobTypeId && !p.IsDeleted)
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new JobTypePriceDto
            {
                Id = p.Id,
                PT_JobTypeId = p.PT_JobTypeId,
                BasePrice = p.BasePrice,
                Currency = p.Currency,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                Notes = p.Notes
            })
            .ToListAsync();
    }

    public async Task<JobTypePriceDto> SetPriceAsync(Guid jobTypeId, SetJobTypePriceDto dto, Guid adminId, string? ipAddress)
    {
        var jobType = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == jobTypeId && !t.IsDeleted)
            ?? throw new InvalidOperationException("El tipo de puesto no existe.");

        if (dto.BasePrice < 0)
            throw new InvalidOperationException("El precio base no puede ser negativo.");

        var effectiveFrom = dto.EffectiveFrom ?? DateTime.UtcNow;

        var current = await _context.PT_JobTypePrices
            .Where(p => p.PT_JobTypeId == jobTypeId && !p.IsDeleted && p.EffectiveTo == null)
            .FirstOrDefaultAsync();
        if (current != null)
        {
            current.EffectiveTo = effectiveFrom;
            current.UpdatedAt = DateTime.UtcNow;
            current.UpdatedBy = adminId;
        }

        var newPrice = new PTJobTypePrice
        {
            PT_JobTypeId = jobTypeId,
            BasePrice = Math.Round(dto.BasePrice, 2),
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EUR" : dto.Currency.Trim().ToUpperInvariant(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Notes = dto.Notes,
            CreatedBy = adminId
        };
        _context.PT_JobTypePrices.Add(newPrice);

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SetJobTypePrice", "PT_JobTypePrices", newPrice.Id,
            $"{{\"jobType\":\"{jobType.Name}\",\"basePrice\":{newPrice.BasePrice}}}", ipAddress);

        return new JobTypePriceDto
        {
            Id = newPrice.Id,
            PT_JobTypeId = newPrice.PT_JobTypeId,
            BasePrice = newPrice.BasePrice,
            Currency = newPrice.Currency,
            EffectiveFrom = newPrice.EffectiveFrom,
            EffectiveTo = newPrice.EffectiveTo,
            Notes = newPrice.Notes
        };
    }

    public async Task<JobTypePriceDto?> GetActivePriceAsync(Guid jobTypeId, DateTime? atDate = null)
    {
        var date = atDate ?? DateTime.UtcNow;
        return await _context.PT_JobTypePrices
            .Where(p => p.PT_JobTypeId == jobTypeId && !p.IsDeleted
                && p.EffectiveFrom <= date && (p.EffectiveTo == null || p.EffectiveTo > date))
            .Select(p => new JobTypePriceDto
            {
                Id = p.Id,
                PT_JobTypeId = p.PT_JobTypeId,
                BasePrice = p.BasePrice,
                Currency = p.Currency,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                Notes = p.Notes
            })
            .FirstOrDefaultAsync();
    }

    // ===== Skills predeterminados por tipo de puesto =====

    public async Task<List<AdminSkillDto>> GetJobTypeSkillsAsync(Guid jobTypeId)
    {
        var exists = await _context.PT_JobTypes.AnyAsync(t => t.Id == jobTypeId && !t.IsDeleted);
        if (!exists) throw new InvalidOperationException("El tipo de puesto no existe.");

        return await _context.PT_JobTypeSkills
            .Where(s => s.PT_JobTypeId == jobTypeId && !s.IsDeleted && !s.Skill.IsDeleted)
            .OrderBy(s => s.Skill.Category).ThenBy(s => s.Skill.Name)
            .Select(s => new AdminSkillDto { Id = s.Skill.Id, Name = s.Skill.Name, Category = s.Skill.Category })
            .ToListAsync();
    }

    public async Task<List<AdminSkillDto>> SetJobTypeSkillsAsync(Guid jobTypeId, List<Guid> skillIds, Guid adminId, string? ipAddress)
    {
        var jobType = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == jobTypeId && !t.IsDeleted)
            ?? throw new InvalidOperationException("El tipo de puesto no existe.");

        // Incluye filas soft-deleted: reactivarlas en vez de insertar una nueva evita chocar con el
        // indice unico (PT_JobTypeId, PT_SkillId, IsDeleted) cuando un skill se destilda y re-tilda.
        var allRows = await _context.PT_JobTypeSkills
            .Where(s => s.PT_JobTypeId == jobTypeId)
            .ToListAsync();

        var requestedIds = skillIds.Distinct().ToList();
        var wantedIds = (await _context.PT_Skills
            .Where(s => requestedIds.Contains(s.Id) && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync()).ToHashSet();
        var rowsBySkillId = allRows.ToDictionary(r => r.PT_SkillId);

        foreach (var row in allRows.Where(r => !r.IsDeleted && !wantedIds.Contains(r.PT_SkillId)))
        {
            row.IsDeleted = true;
            row.DeletedAt = DateTime.UtcNow;
            row.DeletedBy = adminId;
        }

        foreach (var skillId in wantedIds)
        {
            if (rowsBySkillId.TryGetValue(skillId, out var existingRow))
            {
                if (existingRow.IsDeleted)
                {
                    existingRow.IsDeleted = false;
                    existingRow.DeletedAt = null;
                    existingRow.DeletedBy = null;
                    existingRow.UpdatedAt = DateTime.UtcNow;
                    existingRow.UpdatedBy = adminId;
                }
            }
            else
            {
                _context.PT_JobTypeSkills.Add(new PTJobTypeSkill
                {
                    PT_JobTypeId = jobTypeId,
                    PT_SkillId = skillId,
                    CreatedBy = adminId
                });
            }
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SetJobTypeSkills", "PT_JobTypeSkills", jobTypeId,
            $"{{\"jobType\":\"{jobType.Name}\",\"skillCount\":{wantedIds.Count}}}", ipAddress);

        return await GetJobTypeSkillsAsync(jobTypeId);
    }

    public async Task<List<JobTypeOptionDto>> GetActiveJobTypeOptionsAsync()
    {
        return await _context.PT_JobTypes
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Select(t => new JobTypeOptionDto
            {
                Id = t.Id,
                Name = t.Name,
                JobLevelName = t.JobLevel.Name,
                DefaultSkills = t.DefaultSkills
                    .Where(s => !s.IsDeleted && !s.Skill.IsDeleted)
                    .OrderBy(s => s.Skill.Category).ThenBy(s => s.Skill.Name)
                    .Select(s => new AdminSkillDto { Id = s.Skill.Id, Name = s.Skill.Name, Category = s.Skill.Category })
                    .ToList()
            })
            .ToListAsync();
    }
}
