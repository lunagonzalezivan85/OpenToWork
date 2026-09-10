using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class AdminApplicationService : IAdminApplicationService
{
    private readonly AppDbContext _context;

    public AdminApplicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminApplicationDto>> GetByVacancyAsync(Guid vacancyId)
    {
        return await _context.PT_Applications
            .Where(a => a.PT_VacancyId == vacancyId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AdminApplicationDto
            {
                Id = a.Id,
                UserId = a.Candidate.SCUserId,
                CandidateName = a.Candidate.FirstName + " " + a.Candidate.LastName,
                CandidateEmail = a.Candidate.User.Email,
                VacancyTitle = a.Vacancy.Title,
                Status = a.Status,
                ExpectedSalary = a.ExpectedSalary,
                AvailableFromDate = a.AvailableFromDate,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<AdminApplicationDto>> GetApplicationsAsync(int page, int pageSize, int? status)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1_000_000);

        var query = _context.PT_Applications.Where(a => !a.IsDeleted);

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AdminApplicationDto
            {
                Id = a.Id,
                CandidateName = a.Candidate.FirstName + " " + a.Candidate.LastName,
                CandidateEmail = a.Candidate.User.Email,
                VacancyTitle = a.Vacancy.Title,
                Status = a.Status,
                ExpectedSalary = a.ExpectedSalary,
                AvailableFromDate = a.AvailableFromDate,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }
}
