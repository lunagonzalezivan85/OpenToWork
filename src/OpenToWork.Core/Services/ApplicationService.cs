using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _context;

    public ApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationDto> ApplyAsync(Guid candidateId, CreateApplicationDto dto)
    {
        var application = new PTApplication
        {
            PT_CandidateId = candidateId,
            PT_VacancyId = dto.VacancyId,
            CoverLetter = dto.CoverLetter,
            ExpectedSalary = dto.ExpectedSalary,
            AvailableFromDate = dto.AvailableFromDate,
            Status = 0,
            ApplicationSource = 0,
            CreatedBy = candidateId
        };

        _context.PT_Applications.Add(application);
        await _context.SaveChangesAsync();
        return await MapToDtoAsync(application);
    }

    public async Task<IEnumerable<ApplicationDto>> GetApplicationsByCandidateAsync(Guid candidateId)
    {
        var applications = await _context.PT_Applications
            .Include(a => a.Vacancy).ThenInclude(v => v!.Company)
            .Include(a => a.Candidate)
            .Where(a => a.PT_CandidateId == candidateId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var dtos = new List<ApplicationDto>();
        foreach (var a in applications)
            dtos.Add(await MapToDtoAsync(a));
        return dtos;
    }

    public async Task<IEnumerable<ApplicationDto>> GetApplicationsByVacancyAsync(Guid vacancyId, Guid userId)
    {
        var vacancy = await _context.PT_Vacancies
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == vacancyId && !v.IsDeleted);

        if (vacancy == null || vacancy.Company?.SCUserId != userId)
            return new List<ApplicationDto>();

        var applications = await _context.PT_Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy).ThenInclude(v => v!.Company)
            .Where(a => a.PT_VacancyId == vacancyId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var dtos = new List<ApplicationDto>();
        foreach (var a in applications)
            dtos.Add(await MapToDtoAsync(a));
        return dtos;
    }

    public async Task<ApplicationDto?> UpdateApplicationStatusAsync(Guid applicationId, int status, Guid userId)
    {
        var application = await _context.PT_Applications
            .Include(a => a.Candidate)
            .Include(a => a.Vacancy).ThenInclude(v => v!.Company)
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted);

        if (application == null) return null;

        var vacancy = application.Vacancy;
        if (vacancy?.Company?.SCUserId != userId)
            return null;

        application.Status = status;
        application.UpdatedAt = DateTime.UtcNow;
        application.UpdatedBy = userId;
        await _context.SaveChangesAsync();
        return await MapToDtoAsync(application);
    }

    public async Task<bool> HasAlreadyAppliedAsync(Guid candidateId, Guid vacancyId)
    {
        return await _context.PT_Applications
            .AnyAsync(a => a.PT_CandidateId == candidateId && a.PT_VacancyId == vacancyId && !a.IsDeleted);
    }

    private async Task<ApplicationDto> MapToDtoAsync(PTApplication a)
    {
        var candidate = a.Candidate ?? await _context.PT_Candidates.FirstOrDefaultAsync(c => c.Id == a.PT_CandidateId);
        var vacancy = a.Vacancy ?? await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == a.PT_VacancyId);
        var company = vacancy?.Company ?? (vacancy != null ? await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == vacancy.PT_CompanyId) : null);

        return new ApplicationDto
        {
            Id = a.Id,
            CandidateId = a.PT_CandidateId,
            CandidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}" : string.Empty,
            CandidateTitle = candidate?.Title,
            VacancyId = a.PT_VacancyId,
            VacancyTitle = vacancy?.Title ?? string.Empty,
            CompanyName = company?.Name,
            Status = a.Status,
            CoverLetter = a.CoverLetter,
            ExpectedSalary = a.ExpectedSalary,
            AvailableFromDate = a.AvailableFromDate,
            ApplicationSource = a.ApplicationSource,
            CreatedAt = a.CreatedAt,
            ProfileCompletionPercentage = CalculateProfileCompletion(candidate)
        };
    }

    private static int CalculateProfileCompletion(PTCandidate? c)
    {
        if (c == null) return 0;
        int filled = 0;
        int total = 15;
        if (!string.IsNullOrEmpty(c.FirstName)) filled++;
        if (!string.IsNullOrEmpty(c.LastName)) filled++;
        if (!string.IsNullOrEmpty(c.Phone)) filled++;
        if (!string.IsNullOrEmpty(c.Identification)) filled++;
        if (c.BirthDate.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.Country)) filled++;
        if (!string.IsNullOrEmpty(c.City)) filled++;
        if (!string.IsNullOrEmpty(c.Title)) filled++;
        if (!string.IsNullOrEmpty(c.Summary)) filled++;
        if (c.YearsOfExperience.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.LinkedInUrl)) filled++;
        if (!string.IsNullOrEmpty(c.PortfolioUrl)) filled++;
        if (c.Availability.HasValue) filled++;
        if (c.WorkAuthorization.HasValue) filled++;
        if (!string.IsNullOrEmpty(c.CvUrl)) filled++;
        return (int)Math.Round((double)filled / total * 100);
    }
}
