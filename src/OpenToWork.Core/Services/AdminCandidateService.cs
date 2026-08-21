using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using System.Text;

namespace OpenToWork.Core.Services;

public class AdminCandidateService : IAdminCandidateService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public AdminCandidateService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<CandidateConsoleResultDto> GetCandidatesAsync(
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
        bool sortDesc = true)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.SC_Users
            .Where(u => !u.IsDeleted && u.PrimaryRole == 0)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(lower) ||
                (u.Candidate != null && (
                    (u.Candidate.FirstName + " " + u.Candidate.LastName).ToLower().Contains(lower) ||
                    (u.Candidate.Title != null && u.Candidate.Title.ToLower().Contains(lower))
                ))
            );
        }

        if (wizardCompleted.HasValue)
            query = query.Where(u => u.Candidate != null && u.Candidate.WizardCompleted == wizardCompleted.Value);

        if (hasLinkedIn.HasValue)
            query = query.Where(u => u.Candidate != null &&
                (hasLinkedIn.Value ? !string.IsNullOrEmpty(u.Candidate.LinkedInUrl) : string.IsNullOrEmpty(u.Candidate.LinkedInUrl)));

        if (hasPortfolio.HasValue)
            query = query.Where(u => u.Candidate != null &&
                (hasPortfolio.Value ? !string.IsNullOrEmpty(u.Candidate.PortfolioUrl) : string.IsNullOrEmpty(u.Candidate.PortfolioUrl)));

        if (hasCV.HasValue)
            query = query.Where(u => u.Candidate != null &&
                (hasCV.Value ? !string.IsNullOrEmpty(u.Candidate.CvUrl) : string.IsNullOrEmpty(u.Candidate.CvUrl)));

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (skillId.HasValue)
            query = query.Where(u => u.Candidate != null &&
                u.Candidate.CandidateSkills!.Any(cs => cs.PT_SkillId == skillId.Value));

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => sortDesc
                ? query.OrderByDescending(u => u.Candidate!.FirstName).ThenByDescending(u => u.Candidate!.LastName)
                : query.OrderBy(u => u.Candidate!.FirstName).ThenBy(u => u.Candidate!.LastName),
            "email" => sortDesc
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "experience" => sortDesc
                ? query.OrderByDescending(u => u.Candidate!.YearsOfExperience ?? 0)
                : query.OrderBy(u => u.Candidate!.YearsOfExperience ?? 0),
            "skills" => sortDesc
                ? query.OrderByDescending(u => u.Candidate!.CandidateSkills!.Count)
                : query.OrderBy(u => u.Candidate!.CandidateSkills!.Count),
            "applications" => sortDesc
                ? query.OrderByDescending(u => u.Candidate!.Applications!.Count)
                : query.OrderBy(u => u.Candidate!.Applications!.Count),
            _ => sortDesc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new CandidateConsoleDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.Candidate != null ? (u.Candidate.FirstName + " " + u.Candidate.LastName) : u.Email,
                Title = u.Candidate != null ? u.Candidate.Title : null,
                IsActive = u.IsActive,
                WizardCompleted = u.Candidate != null && u.Candidate.WizardCompleted,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                CompletedAt = u.Candidate != null ? u.Candidate.CompletedAt : null,
                HasLinkedIn = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.LinkedInUrl),
                HasPortfolio = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.PortfolioUrl),
                HasCV = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.CvUrl),
                YearsOfExperience = u.Candidate != null ? u.Candidate.YearsOfExperience : null,
                Country = u.Candidate != null ? u.Candidate.Country : null,
                City = u.Candidate != null ? u.Candidate.City : null,
                Availability = u.Candidate != null ? u.Candidate.Availability : null,
                SkillCount = u.Candidate != null ? u.Candidate.CandidateSkills!.Count : 0,
                ExperienceCount = u.Candidate != null ? u.Candidate.Experiences!.Count : 0,
                ApplicationCount = u.Candidate != null ? u.Candidate.Applications!.Count : 0,
                TopSkills = new List<string>()
            })
            .ToListAsync();

        var candidateIds = items.Select(i => i.Id).ToList();
        var candidateSkillData = await _context.PT_CandidateSkills
            .Where(cs => candidateIds.Contains(cs.PT_CandidateId) && !cs.IsDeleted)
            .Include(cs => cs.Skill)
            .ToListAsync();

        var skillsByCandidate = candidateSkillData
            .GroupBy(cs => cs.PT_CandidateId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(cs => cs.ProficiencyLevel ?? 0)
                      .Take(3)
                      .Select(cs => cs.Skill?.Name ?? "")
                      .ToList()
            );

        foreach (var item in items)
        {
            if (skillsByCandidate.TryGetValue(item.Id, out var skills))
                item.TopSkills = skills;
        }

        var allCandidates = _context.SC_Users
            .Where(u => !u.IsDeleted && u.PrimaryRole == 0)
            .Include(u => u.Candidate)
            .AsQueryable();

        var stats = new CandidateConsoleStatsDto
        {
            TotalCandidates = await allCandidates.CountAsync(),
            EvaluatedProfiles = await allCandidates.CountAsync(u => u.Candidate != null && u.Candidate.WizardCompleted),
            PendingProfiles = await allCandidates.CountAsync(u => u.Candidate != null && !u.Candidate.WizardCompleted),
            WithLinkedIn = await allCandidates.CountAsync(u => u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.LinkedInUrl)),
            WithPortfolio = await allCandidates.CountAsync(u => u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.PortfolioUrl)),
            WithCV = await allCandidates.CountAsync(u => u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.CvUrl)),
            WithApplications = await allCandidates.CountAsync(u => u.Candidate != null && u.Candidate.Applications!.Any()),
            ActiveCandidates = await allCandidates.CountAsync(u => u.IsActive),
            InactiveCandidates = await allCandidates.CountAsync(u => !u.IsActive)
        };

        return new CandidateConsoleResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Stats = stats
        };
    }

    public async Task<bool> BulkActivateAsync(List<Guid> ids, Guid adminId, string? ipAddress)
    {
        var users = await _context.SC_Users
            .Where(u => ids.Contains(u.Id) && !u.IsDeleted && u.PrimaryRole == 0)
            .ToListAsync();

        if (users.Count == 0) return false;

        foreach (var user in users)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = adminId;
        }

        await _context.SaveChangesAsync();

        foreach (var user in users)
            await _auditLog.LogAsync(adminId, "BulkActivateCandidate", "SC_Users", user.Id, null, ipAddress);

        return true;
    }

    public async Task<bool> BulkDeactivateAsync(List<Guid> ids, Guid adminId, string? ipAddress)
    {
        var users = await _context.SC_Users
            .Where(u => ids.Contains(u.Id) && !u.IsDeleted && u.PrimaryRole == 0)
            .ToListAsync();

        if (users.Count == 0) return false;

        foreach (var user in users)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = adminId;
        }

        await _context.SaveChangesAsync();

        foreach (var user in users)
            await _auditLog.LogAsync(adminId, "BulkDeactivateCandidate", "SC_Users", user.Id, null, ipAddress);

        return true;
    }

    public async Task<string> ExportCandidatesCsvAsync()
    {
        var candidates = await _context.SC_Users
            .Where(u => !u.IsDeleted && u.PrimaryRole == 0)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new CandidateConsoleDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.Candidate != null ? (u.Candidate.FirstName + " " + u.Candidate.LastName) : u.Email,
                Title = u.Candidate != null ? u.Candidate.Title : null,
                IsActive = u.IsActive,
                WizardCompleted = u.Candidate != null && u.Candidate.WizardCompleted,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                CompletedAt = u.Candidate != null ? u.Candidate.CompletedAt : null,
                HasLinkedIn = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.LinkedInUrl),
                HasPortfolio = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.PortfolioUrl),
                HasCV = u.Candidate != null && !string.IsNullOrEmpty(u.Candidate.CvUrl),
                YearsOfExperience = u.Candidate != null ? u.Candidate.YearsOfExperience : null,
                Country = u.Candidate != null ? u.Candidate.Country : null,
                City = u.Candidate != null ? u.Candidate.City : null,
                Availability = u.Candidate != null ? u.Candidate.Availability : null,
                SkillCount = u.Candidate != null ? u.Candidate.CandidateSkills!.Count : 0,
                ExperienceCount = u.Candidate != null ? u.Candidate.Experiences!.Count : 0,
                ApplicationCount = u.Candidate != null ? u.Candidate.Applications!.Count : 0,
                TopSkills = new List<string>()
            })
            .ToListAsync();

        var exportCandidateIds = candidates.Select(c => c.Id).ToList();
        var exportSkillData = await _context.PT_CandidateSkills
            .Where(cs => exportCandidateIds.Contains(cs.PT_CandidateId) && !cs.IsDeleted)
            .Include(cs => cs.Skill)
            .ToListAsync();

        var exportSkillsMap = exportSkillData
            .GroupBy(cs => cs.PT_CandidateId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(cs => cs.ProficiencyLevel ?? 0)
                      .Take(5)
                      .Select(cs => cs.Skill?.Name ?? "")
                      .ToList()
            );

        foreach (var c in candidates)
        {
            if (exportSkillsMap.TryGetValue(c.Id, out var skills))
                c.TopSkills = skills;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id,Email,FullName,Title,IsActive,WizardCompleted,CreatedAt,LastLoginAt,CompletedAt,HasLinkedIn,HasPortfolio,HasCV,YearsOfExperience,Country,City,SkillCount,ExperienceCount,ApplicationCount,TopSkills");

        foreach (var c in candidates)
        {
            sb.AppendLine(string.Join(",",
                c.Id,
                CsvEscape(c.Email),
                CsvEscape(c.FullName),
                CsvEscape(c.Title ?? ""),
                c.IsActive,
                c.WizardCompleted,
                c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                c.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                c.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                c.HasLinkedIn,
                c.HasPortfolio,
                c.HasCV,
                c.YearsOfExperience ?? 0,
                CsvEscape(c.Country ?? ""),
                CsvEscape(c.City ?? ""),
                c.SkillCount,
                c.ExperienceCount,
                c.ApplicationCount,
                CsvEscape(string.Join("; ", c.TopSkills))
            ));
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
