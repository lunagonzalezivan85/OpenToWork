using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminVacancyService : IAdminVacancyService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public AdminVacancyService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<List<AdminVacancyDto>> GetVacanciesAsync(int page, int pageSize, int? status)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1_000_000);

        var permanentQuery = _context.PT_Vacancies
            .Where(v => !v.IsDeleted)
            .Select(v => new AdminVacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                CompanyName = v.Company.Name,
                Location = v.Location,
                ContractType = v.ContractType,
                WorkMode = v.WorkMode,
                Status = v.Status,
                IsTemporary = false,
                PublishedAt = v.PublishedAt,
                ClosedAt = v.ClosedAt,
                ExpiresAt = null,
                ViewsCount = v.ViewsCount
            });

        var tempQuery = _context.PT_TempVacancies
            .Where(v => !v.IsDeleted)
            .Select(v => new AdminVacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                CompanyName = null,
                Location = v.Location,
                ContractType = v.ContractType,
                WorkMode = v.WorkMode,
                Status = v.IsPublished ? (int)VacancyStatus.Active : (int)VacancyStatus.Draft,
                IsTemporary = true,
                PublishedAt = null,
                ClosedAt = null,
                ExpiresAt = v.ExpiresAt,
                ViewsCount = 0
            });

        // Both sides assign the exact same set of AdminVacancyDto properties (with explicit
        // nulls for the ones that don't apply) so EF/Pomelo can translate this Concat into a
        // single UNION ALL query with server-side ORDER BY/LIMIT/OFFSET instead of loading
        // both tables into memory.
        var combined = permanentQuery.Concat(tempQuery);

        if (status.HasValue) combined = combined.Where(v => v.Status == status.Value);

        return await combined
            .OrderByDescending(v => v.PublishedAt ?? v.ExpiresAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> ModerateAsync(Guid id, int status, Guid adminId, string? ipAddress)
    {
        var vacancy = await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (vacancy != null)
        {
            vacancy.Status = status;
            vacancy.UpdatedAt = DateTime.UtcNow;
            vacancy.UpdatedBy = adminId;
            if (status == (int)VacancyStatus.Closed) vacancy.ClosedAt = DateTime.UtcNow;
            if (status == (int)VacancyStatus.Active && vacancy.PublishedAt == null) vacancy.PublishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(adminId, "ModerateVacancy", "PT_Vacancies", id, $"{{\"status\":{status}}}", ipAddress);
            return true;
        }

        var tempVacancy = await _context.PT_TempVacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (tempVacancy != null)
        {
            // PT_TempVacancy only has an IsPublished flag, with no field to represent "Closed" the way
            // PT_Vacancy.Status does. Treat Closed as terminal: unpublish and soft-delete so it stops
            // appearing (rather than falling back to a state indistinguishable from "never reviewed").
            tempVacancy.IsPublished = status == (int)VacancyStatus.Active;
            tempVacancy.UpdatedAt = DateTime.UtcNow;
            tempVacancy.UpdatedBy = adminId;
            if (status == (int)VacancyStatus.Closed)
            {
                tempVacancy.IsDeleted = true;
                tempVacancy.DeletedAt = DateTime.UtcNow;
                tempVacancy.DeletedBy = adminId;
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(adminId, "ModerateVacancy", "PT_TempVacancies", id, $"{{\"status\":{status}}}", ipAddress);
            return true;
        }

        return false;
    }
}
