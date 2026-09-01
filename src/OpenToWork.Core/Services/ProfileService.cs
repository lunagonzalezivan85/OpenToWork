using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateProfileDto?> GetProfileAsync(Guid userId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) return null;

        return MapToProfileDto(candidate);
    }

    public async Task<CandidateProfileDto?> GetCandidateByIdAsync(Guid candidateId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);

        if (candidate == null) return null;

        return MapToProfileDto(candidate);
    }

    public async Task<CandidateProfileDto?> UpdateProfileAsync(Guid userId, UpdateCandidateProfileDto dto)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) return null;

        if (dto.Title != null) candidate.Title = dto.Title;
        if (dto.Summary != null) candidate.Summary = dto.Summary;
        if (dto.YearsOfExperience.HasValue) candidate.YearsOfExperience = dto.YearsOfExperience;
        if (dto.LinkedInUrl != null) candidate.LinkedInUrl = dto.LinkedInUrl;
        if (dto.PortfolioUrl != null) candidate.PortfolioUrl = dto.PortfolioUrl;
        if (dto.Availability.HasValue) candidate.Availability = dto.Availability;
        if (dto.WorkAuthorization.HasValue) candidate.WorkAuthorization = dto.WorkAuthorization;
        if (dto.IsProfilePublic.HasValue) candidate.IsProfilePublic = dto.IsProfilePublic.Value;
        if (dto.CvUrl != null) candidate.CvUrl = dto.CvUrl;
        if (dto.ProfilePictureUrl != null) candidate.ProfilePictureUrl = dto.ProfilePictureUrl;
        if (dto.Phone != null) candidate.Phone = dto.Phone;
        if (dto.Identification != null) candidate.Identification = dto.Identification;
        if (dto.BirthDate.HasValue) candidate.BirthDate = dto.BirthDate;
        if (dto.Country != null) candidate.Country = dto.Country;
        if (dto.City != null) candidate.City = dto.City;
        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        var updated = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .FirstOrDefaultAsync(c => c.Id == candidate.Id);

        return updated != null ? MapToProfileDto(updated) : null;
    }

    public async Task<CandidateExperienceDto> AddExperienceAsync(Guid userId, CreateExperienceDto dto)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var experience = new PTCandidateExperience
        {
            PT_CandidateId = candidate.Id,
            CompanyName = dto.CompanyName,
            JobTitle = dto.JobTitle,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsCurrentJob = dto.IsCurrentJob,
            Location = dto.Location,
            CreatedBy = userId
        };

        _context.PT_CandidateExperiences.Add(experience);
        await _context.SaveChangesAsync();
        return MapToExperienceDto(experience);
    }

    public async Task<CandidateExperienceDto?> UpdateExperienceAsync(Guid experienceId, UpdateExperienceDto dto, Guid userId)
    {
        var experience = await _context.PT_CandidateExperiences
            .FirstOrDefaultAsync(e => e.Id == experienceId && !e.IsDeleted);

        if (experience == null) return null;

        if (dto.CompanyName != null) experience.CompanyName = dto.CompanyName;
        if (dto.JobTitle != null) experience.JobTitle = dto.JobTitle;
        if (dto.Description != null) experience.Description = dto.Description;
        if (dto.StartDate.HasValue) experience.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) experience.EndDate = dto.EndDate;
        if (dto.IsCurrentJob.HasValue) experience.IsCurrentJob = dto.IsCurrentJob.Value;
        if (dto.Location != null) experience.Location = dto.Location;
        experience.UpdatedAt = DateTime.UtcNow;
        experience.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return MapToExperienceDto(experience);
    }

    public async Task<bool> DeleteExperienceAsync(Guid experienceId, Guid userId)
    {
        var experience = await _context.PT_CandidateExperiences
            .FirstOrDefaultAsync(e => e.Id == experienceId && !e.IsDeleted);

        if (experience == null) return false;

        experience.IsDeleted = true;
        experience.DeletedAt = DateTime.UtcNow;
        experience.DeletedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CandidateEducationDto> AddEducationAsync(Guid userId, CreateEducationDto dto)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var education = new PTCandidateEducation
        {
            PT_CandidateId = candidate.Id,
            Institution = dto.Institution,
            Degree = dto.Degree,
            FieldOfStudy = dto.FieldOfStudy,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsInProgress = dto.IsInProgress,
            CreatedBy = userId
        };

        _context.PT_CandidateEducations.Add(education);
        await _context.SaveChangesAsync();
        return MapToEducationDto(education);
    }

    public async Task<CandidateEducationDto?> UpdateEducationAsync(Guid educationId, UpdateEducationDto dto, Guid userId)
    {
        var education = await _context.PT_CandidateEducations
            .FirstOrDefaultAsync(e => e.Id == educationId && !e.IsDeleted);

        if (education == null) return null;

        if (dto.Institution != null) education.Institution = dto.Institution;
        if (dto.Degree != null) education.Degree = dto.Degree;
        if (dto.FieldOfStudy != null) education.FieldOfStudy = dto.FieldOfStudy;
        if (dto.StartDate.HasValue) education.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) education.EndDate = dto.EndDate;
        if (dto.IsInProgress.HasValue) education.IsInProgress = dto.IsInProgress.Value;
        education.UpdatedAt = DateTime.UtcNow;
        education.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return MapToEducationDto(education);
    }

    public async Task<bool> DeleteEducationAsync(Guid educationId, Guid userId)
    {
        var education = await _context.PT_CandidateEducations
            .FirstOrDefaultAsync(e => e.Id == educationId && !e.IsDeleted);

        if (education == null) return false;

        education.IsDeleted = true;
        education.DeletedAt = DateTime.UtcNow;
        education.DeletedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CandidateCertificationDto> AddCertificationAsync(Guid userId, CreateCertificationDto dto)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var certification = new PTCandidateCertification
        {
            PT_CandidateId = candidate.Id,
            Name = dto.Name,
            Issuer = dto.Issuer,
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            CredentialId = dto.CredentialId,
            CredentialUrl = dto.CredentialUrl,
            CreatedBy = userId
        };

        _context.PT_CandidateCertifications.Add(certification);
        await _context.SaveChangesAsync();
        return MapToCertificationDto(certification);
    }

    public async Task<CandidateCertificationDto?> UpdateCertificationAsync(Guid certificationId, UpdateCertificationDto dto, Guid userId)
    {
        var certification = await _context.PT_CandidateCertifications
            .FirstOrDefaultAsync(c => c.Id == certificationId && !c.IsDeleted);

        if (certification == null) return null;

        if (dto.Name != null) certification.Name = dto.Name;
        if (dto.Issuer != null) certification.Issuer = dto.Issuer;
        if (dto.IssueDate.HasValue) certification.IssueDate = dto.IssueDate;
        if (dto.ExpiryDate.HasValue) certification.ExpiryDate = dto.ExpiryDate;
        if (dto.CredentialId != null) certification.CredentialId = dto.CredentialId;
        if (dto.CredentialUrl != null) certification.CredentialUrl = dto.CredentialUrl;
        certification.UpdatedAt = DateTime.UtcNow;
        certification.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return MapToCertificationDto(certification);
    }

    public async Task<bool> DeleteCertificationAsync(Guid certificationId, Guid userId)
    {
        var certification = await _context.PT_CandidateCertifications
            .FirstOrDefaultAsync(c => c.Id == certificationId && !c.IsDeleted);

        if (certification == null) return false;

        certification.IsDeleted = true;
        certification.DeletedAt = DateTime.UtcNow;
        certification.DeletedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CandidateProfileDto?> ApplyCvDataAsync(Guid userId, CvParseResultDto parsed, string cvUrl)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .Include(c => c.CandidateSkills)
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) return null;

        candidate.CvUrl = cvUrl;
        if (!string.IsNullOrEmpty(parsed.Title)) candidate.Title = parsed.Title;
        if (!string.IsNullOrEmpty(parsed.Summary)) candidate.Summary = parsed.Summary;
        if (!string.IsNullOrEmpty(parsed.City)) candidate.City = parsed.City;
        if (!string.IsNullOrEmpty(parsed.Country)) candidate.Country = parsed.Country;
        if (!string.IsNullOrEmpty(parsed.LinkedInUrl)) candidate.LinkedInUrl = parsed.LinkedInUrl;
        if (!string.IsNullOrEmpty(parsed.PortfolioUrl)) candidate.PortfolioUrl = parsed.PortfolioUrl;
        if (parsed.YearsOfExperience.HasValue) candidate.YearsOfExperience = parsed.YearsOfExperience;

        if (parsed.Availability != null)
        {
            var availLower = parsed.Availability.ToLowerInvariant();
            if (availLower.Contains("inmediata") || availLower.Contains("immediate"))
                candidate.Availability = 0;
            else if (availLower.Contains("dos semanas") || availLower.Contains("two weeks"))
                candidate.Availability = 1;
            else if (availLower.Contains("un mes") || availLower.Contains("one month"))
                candidate.Availability = 2;
            else if (availLower.Contains("no disponible") || availLower.Contains("not available"))
                candidate.Availability = 3;
        }

        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedBy = userId;

        foreach (var exp in candidate.Experiences.Where(e => !e.IsDeleted))
        {
            exp.IsDeleted = true;
            exp.DeletedAt = DateTime.UtcNow;
            exp.DeletedBy = userId;
        }

        foreach (var edu in candidate.Educations.Where(e => !e.IsDeleted))
        {
            edu.IsDeleted = true;
            edu.DeletedAt = DateTime.UtcNow;
            edu.DeletedBy = userId;
        }

        foreach (var cert in candidate.Certifications.Where(c => !c.IsDeleted))
        {
            cert.IsDeleted = true;
            cert.DeletedAt = DateTime.UtcNow;
            cert.DeletedBy = userId;
        }

        foreach (var cs in candidate.CandidateSkills.Where(s => !s.IsDeleted))
        {
            cs.IsDeleted = true;
            cs.DeletedAt = DateTime.UtcNow;
            cs.DeletedBy = userId;
        }

        foreach (var exp in parsed.Experiences)
        {
            if (string.IsNullOrEmpty(exp.JobTitle) || string.IsNullOrEmpty(exp.CompanyName)) continue;

            var startDate = TryParseDate(exp.StartDate) ?? DateTime.UtcNow;
            var endDate = TryParseDate(exp.EndDate);

            _context.PT_CandidateExperiences.Add(new PTCandidateExperience
            {
                PT_CandidateId = candidate.Id,
                JobTitle = exp.JobTitle,
                CompanyName = exp.CompanyName,
                Description = exp.Description,
                Location = exp.Location,
                StartDate = startDate,
                EndDate = exp.IsCurrentJob ? null : endDate,
                IsCurrentJob = exp.IsCurrentJob,
                CreatedBy = userId
            });
        }

        foreach (var edu in parsed.Educations)
        {
            if (string.IsNullOrEmpty(edu.Institution) || string.IsNullOrEmpty(edu.Degree)) continue;

            _context.PT_CandidateEducations.Add(new PTCandidateEducation
            {
                PT_CandidateId = candidate.Id,
                Institution = edu.Institution,
                Degree = edu.Degree,
                FieldOfStudy = edu.FieldOfStudy,
                StartDate = TryParseDate(edu.StartDate),
                EndDate = edu.IsInProgress ? null : TryParseDate(edu.EndDate),
                IsInProgress = edu.IsInProgress,
                CreatedBy = userId
            });
        }

        foreach (var cert in parsed.Certifications)
        {
            if (string.IsNullOrEmpty(cert.Name)) continue;

            _context.PT_CandidateCertifications.Add(new PTCandidateCertification
            {
                PT_CandidateId = candidate.Id,
                Name = cert.Name,
                Issuer = cert.Issuer,
                IssueDate = TryParseDate(cert.IssueDate),
                ExpiryDate = TryParseDate(cert.ExpiryDate),
                CreatedBy = userId
            });
        }

        var newSkills = new List<PTSkill>();
        foreach (var skillName in parsed.Skills)
        {
            if (string.IsNullOrWhiteSpace(skillName)) continue;

            var existingSkill = await _context.PT_Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());
            var skillId = existingSkill?.Id ?? Guid.Empty;

            if (existingSkill == null)
            {
                var newSkill = new PTSkill { Name = skillName.Trim(), CreatedBy = userId };
                _context.PT_Skills.Add(newSkill);
                newSkills.Add(newSkill);
            }
            else
            {
                _context.PT_CandidateSkills.Add(new PTCandidateSkill
                {
                    PT_CandidateId = candidate.Id,
                    PT_SkillId = skillId,
                    CreatedBy = userId
                });
            }
        }

        await _context.SaveChangesAsync();

        foreach (var newSkill in newSkills)
        {
            _context.PT_CandidateSkills.Add(new PTCandidateSkill
            {
                PT_CandidateId = candidate.Id,
                PT_SkillId = newSkill.Id,
                CreatedBy = userId
            });
        }

        await _context.SaveChangesAsync();

        var updated = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .FirstOrDefaultAsync(c => c.Id == candidate.Id);

        return updated != null ? MapToProfileDto(updated) : null;
    }

    private static DateTime? TryParseDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return null;

        if (DateTime.TryParseExact(dateStr, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dateFull))
            return dateFull;

        if (DateTime.TryParse(dateStr, out var generalDate))
            return generalDate;

        if (int.TryParse(dateStr, out var year) && year > 1900 && year < 2100)
            return new DateTime(year, 1, 1);

        return null;
    }

    private static CandidateProfileDto MapToProfileDto(PTCandidate c) => new()
    {
        Id = c.Id,
        UserId = c.SCUserId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Identification = c.Identification,
        Phone = c.Phone,
        BirthDate = c.BirthDate,
        Gender = c.Gender,
        Title = c.Title,
        Summary = c.Summary,
        CvUrl = c.CvUrl,
        ProfilePictureUrl = c.ProfilePictureUrl,
        Country = c.Country,
        City = c.City,
        Address = c.Address,
        YearsOfExperience = c.YearsOfExperience,
        LinkedInUrl = c.LinkedInUrl,
        PortfolioUrl = c.PortfolioUrl,
        Availability = c.Availability,
        WorkAuthorization = c.WorkAuthorization,
        IsProfilePublic = c.IsProfilePublic,
        Experiences = c.Experiences.Where(e => !e.IsDeleted).Select(MapToExperienceDto).ToList(),
        Educations = c.Educations.Where(e => !e.IsDeleted).Select(MapToEducationDto).ToList(),
        Certifications = c.Certifications.Where(c => !c.IsDeleted).Select(MapToCertificationDto).ToList(),
        Skills = c.CandidateSkills.Where(cs => !cs.IsDeleted && !cs.Skill.IsDeleted).Select(cs => new CandidateSkillDto
        {
            Id = cs.Id,
            Name = cs.Skill.Name,
            Category = cs.Skill.Category,
            ProficiencyLevel = cs.ProficiencyLevel
        }).ToList()
    };

    private static CandidateExperienceDto MapToExperienceDto(PTCandidateExperience e) => new()
    {
        Id = e.Id,
        CandidateId = e.PT_CandidateId,
        CompanyName = e.CompanyName,
        JobTitle = e.JobTitle,
        Description = e.Description,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        IsCurrentJob = e.IsCurrentJob,
        Location = e.Location
    };

    private static CandidateEducationDto MapToEducationDto(PTCandidateEducation e) => new()
    {
        Id = e.Id,
        CandidateId = e.PT_CandidateId,
        Institution = e.Institution,
        Degree = e.Degree,
        FieldOfStudy = e.FieldOfStudy,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        IsInProgress = e.IsInProgress
    };

    private static CandidateCertificationDto MapToCertificationDto(PTCandidateCertification c) => new()
    {
        Id = c.Id,
        CandidateId = c.PT_CandidateId,
        Name = c.Name,
        Issuer = c.Issuer,
        IssueDate = c.IssueDate,
        ExpiryDate = c.ExpiryDate,
        CredentialId = c.CredentialId,
        CredentialUrl = c.CredentialUrl
    };
}
