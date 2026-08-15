using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync()
    {
        var vacanciesByStatus = await _context.PT_Vacancies
            .Where(v => !v.IsDeleted)
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var applicationsByStatus = await _context.PT_Applications
            .Where(a => !a.IsDeleted)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var companiesWithVacancies = await _context.PT_Vacancies
            .Where(v => !v.IsDeleted)
            .Select(v => v.PT_CompanyId)
            .Distinct()
            .CountAsync();

        return new DashboardMetricsDto
        {
            TotalUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted),
            ActiveUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.IsActive),
            TotalCandidates = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted),
            TotalCompanies = await _context.PT_Companies.CountAsync(c => !c.IsDeleted),
            TotalPermanentVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted),
            TotalTempVacancies = await _context.PT_TempVacancies.CountAsync(v => !v.IsDeleted),
            VacanciesByStatus = vacanciesByStatus.ToDictionary(x => ((VacancyStatus)x.Status).ToString(), x => x.Count),
            ApplicationsByStatus = applicationsByStatus.ToDictionary(x => ((ApplicationStatus)x.Status).ToString(), x => x.Count),
            TotalSkills = await _context.PT_Skills.CountAsync(s => !s.IsDeleted),
            TotalAuditLogEntries = await _context.AD_AuditLogs.CountAsync(a => !a.IsDeleted),

            EvaluatedProfiles = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && c.WizardCompleted),
            PendingProfiles = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !c.WizardCompleted),
            ProfilesWithScores = 0,
            OpenVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Active),
            ClosedVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Closed),
            DraftVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Draft),
            CompaniesWithVacancies = companiesWithVacancies,
            CompaniesWithoutVacancies = await _context.PT_Companies.CountAsync(c => !c.IsDeleted) - companiesWithVacancies,
            NonAdminUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole != (int)UserRole.Admin),
            NonAdminCandidates = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole == (int)UserRole.Candidate),
            NonAdminCompanies = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole == (int)UserRole.Company),
            CandidatesWithLinkedIn = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.LinkedInUrl)),
            CandidatesWithPortfolio = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.PortfolioUrl)),
            CandidatesWithCV = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.CvUrl))
        };
    }
}
