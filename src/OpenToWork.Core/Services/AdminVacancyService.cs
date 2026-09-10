using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
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

    public async Task<AdminVacancyDto?> CreateAsync(AdminCreateVacancyDto dto, Guid adminId, string? ipAddress)
    {
        var companyExists = await _context.PT_Companies.AnyAsync(c => c.Id == dto.CompanyId && !c.IsDeleted);
        if (!companyExists) return null;

        var vacancy = new PTVacancy
        {
            Id = Guid.NewGuid(),
            PT_CompanyId = dto.CompanyId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Requirements = dto.Requirements,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            Location = dto.Location,
            ContractType = dto.ContractType,
            WorkMode = dto.WorkMode,
            Category = dto.Category,
            ExperienceLevel = dto.ExperienceLevel,
            EnglishLevel = dto.EnglishLevel,
            RequiredApplicants = dto.RequiredApplicants,
            YearsExperience = dto.YearsExperience,
            Status = (int)VacancyStatus.Active,
            PublishedAt = DateTime.UtcNow,
            CreatedBy = adminId
        };

        _context.PT_Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CreateVacancy", "PT_Vacancies", vacancy.Id, $"{{\"title\":\"{dto.Title}\"}}", ipAddress);

        return new AdminVacancyDto
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            CompanyName = (await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == dto.CompanyId))?.Name,
            Location = vacancy.Location,
            ContractType = vacancy.ContractType,
            WorkMode = vacancy.WorkMode,
            Status = vacancy.Status,
            IsTemporary = false,
            PublishedAt = vacancy.PublishedAt,
            ViewsCount = 0
        };
    }

    public async Task<VacancyDto?> GetByIdAsync(Guid id)
    {
        return await _context.PT_Vacancies
            .Where(v => v.Id == id && !v.IsDeleted)
            .Select(v => new VacancyDto
            {
                Id = v.Id,
                CompanyId = v.PT_CompanyId,
                CompanyName = v.Company.Name,
                Title = v.Title,
                Description = v.Description,
                Requirements = v.Requirements,
                SalaryMin = v.SalaryMin,
                SalaryMax = v.SalaryMax,
                Location = v.Location,
                ContractType = v.ContractType,
                WorkMode = v.WorkMode,
                Category = v.Category,
                ExperienceLevel = v.ExperienceLevel,
                EnglishLevel = v.EnglishLevel,
                RequiredApplicants = v.RequiredApplicants,
                YearsExperience = v.YearsExperience,
                Status = v.Status,
                PublishedAt = v.PublishedAt,
                ViewsCount = v.ViewsCount
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CreateApplicationAsync(Guid vacancyId, AdminCreateApplicationDto dto, Guid adminId, string? ipAddress)
    {
        var vacancyExists = await _context.PT_Vacancies.AnyAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (!vacancyExists) return false;

        var candidateExists = await _context.PT_Candidates.AnyAsync(c => c.Id == dto.CandidateId && !c.IsDeleted);
        if (!candidateExists) return false;

        var alreadyApplied = await _context.PT_Applications
            .AnyAsync(a => a.PT_VacancyId == vacancyId && a.PT_CandidateId == dto.CandidateId && !a.IsDeleted);
        if (alreadyApplied) return false;

        var application = new PTApplication
        {
            PT_CandidateId = dto.CandidateId,
            PT_VacancyId = vacancyId,
            Status = (int)ApplicationStatus.Pending,
            ApplicationSource = (int)ApplicationSource.AdminCurated,
            CreatedBy = adminId
        };

        _context.PT_Applications.Add(application);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "CreateApplication", "PT_Applications", application.Id,
            $"{{\"candidateId\":\"{dto.CandidateId}\",\"vacancyId\":\"{vacancyId}\",\"source\":\"AdminCurated\"}}", ipAddress);
        return true;
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
