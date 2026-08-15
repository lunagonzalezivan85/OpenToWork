using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public AdminUserService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(int page, int pageSize, int? role, bool? isActive)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1_000_000);

        var query = _context.SC_Users.Where(u => !u.IsDeleted);

        if (role.HasValue) query = query.Where(u => u.PrimaryRole == role.Value);
        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                PrimaryRole = u.PrimaryRole,
                EmailVerified = u.EmailVerified,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                CandidateName = u.Candidate != null ? (u.Candidate.FirstName + " " + u.Candidate.LastName) : null,
                WizardCompleted = u.Candidate != null ? u.Candidate.WizardCompleted : null,
                HasLinkedIn = u.Candidate != null ? !string.IsNullOrEmpty(u.Candidate.LinkedInUrl) : null,
                HasPortfolio = u.Candidate != null ? !string.IsNullOrEmpty(u.Candidate.PortfolioUrl) : null,
                HasCV = u.Candidate != null ? !string.IsNullOrEmpty(u.Candidate.CvUrl) : null,
                HasScore = false
            })
            .ToListAsync();
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        return user == null ? null : ToDto(user);
    }

    public async Task<AdminUserProfileDto?> GetUserProfileAsync(Guid id)
    {
        var user = await _context.SC_Users
            .Include(u => u.Candidate!)
                .ThenInclude(c => c.CandidateSkills!)
                    .ThenInclude(cs => cs.Skill)
            .Include(u => u.Candidate!)
                .ThenInclude(c => c.Experiences)
            .Include(u => u.Candidate!)
                .ThenInclude(c => c.Educations)
            .Include(u => u.Candidate!)
                .ThenInclude(c => c.Certifications)
            .Include(u => u.Company!)
                .ThenInclude(c => c.Vacancies)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null) return null;

        var dto = new AdminUserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            PrimaryRole = user.PrimaryRole,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        if (user.Candidate != null)
        {
            var c = user.Candidate;
            dto.CandidateName = $"{c.FirstName} {c.LastName}";
            dto.Title = c.Title;
            dto.Summary = c.Summary;
            dto.Phone = c.Phone;
            dto.Identification = c.Identification;
            dto.BirthDate = c.BirthDate;
            dto.Gender = c.Gender;
            dto.Country = c.Country;
            dto.City = c.City;
            dto.Address = c.Address;
            dto.YearsOfExperience = c.YearsOfExperience;
            dto.LinkedInUrl = c.LinkedInUrl;
            dto.PortfolioUrl = c.PortfolioUrl;
            dto.CvUrl = c.CvUrl;
            dto.ProfilePictureUrl = c.ProfilePictureUrl;
            dto.WizardCompleted = c.WizardCompleted;
            dto.Availability = c.Availability;
            dto.WorkAuthorization = c.WorkAuthorization;
            dto.IsProfilePublic = c.IsProfilePublic;
            dto.CompletedAt = c.CompletedAt;
            dto.Skills = c.CandidateSkills?.Select(cs => new AdminCandidateSkillDto
            {
                Name = cs.Skill?.Name ?? "",
                Category = cs.Skill?.Category,
                ProficiencyLevel = cs.ProficiencyLevel
            }).ToList() ?? new();
            dto.Experiences = c.Experiences?.Select(e => new AdminCandidateExperienceDto
            {
                CompanyName = e.CompanyName,
                JobTitle = e.JobTitle,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrentJob = e.IsCurrentJob,
                Location = e.Location
            }).ToList() ?? new();
            dto.Educations = c.Educations?.Select(e => new AdminCandidateEducationDto
            {
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsInProgress = e.IsInProgress
            }).ToList() ?? new();
            dto.Certifications = c.Certifications?.Select(c2 => new AdminCandidateCertificationDto
            {
                Name = c2.Name,
                Issuer = c2.Issuer,
                IssueDate = c2.IssueDate,
                ExpiryDate = c2.ExpiryDate,
                CredentialId = c2.CredentialId,
                CredentialUrl = c2.CredentialUrl
            }).ToList() ?? new();
        }

        if (user.Company != null)
        {
            var co = user.Company;
            dto.CompanyName = co.Name;
            dto.CompanyDescription = co.Description;
            dto.Website = co.Website;
            dto.LogoUrl = co.LogoUrl;
            dto.Country = co.Country;
            dto.City = co.City;
            dto.Address = co.Address;
            dto.Industry = co.Industry;
            dto.CompanySize = co.CompanySize;
            dto.ContactEmail = co.ContactEmail;
            dto.ContactPhone = co.ContactPhone;
            dto.CompanyLinkedInUrl = co.LinkedInUrl;
            dto.IsVerified = co.IsVerified;
            dto.VacancyCount = co.Vacancies?.Count(v => !v.IsDeleted) ?? 0;
        }

        return dto;
    }

    public async Task<bool> ActivateAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "ActivateUser", "SC_Users", id, null, ipAddress);
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "DeactivateUser", "SC_Users", id, null, ipAddress);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "DeleteUser", "SC_Users", id, null, ipAddress);
        return true;
    }

    private static AdminUserDto ToDto(Models.Entities.SCUser u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        PrimaryRole = u.PrimaryRole,
        EmailVerified = u.EmailVerified,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt
    };
}
