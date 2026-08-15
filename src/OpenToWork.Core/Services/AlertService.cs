using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;

    public AlertService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlertDto>> GetAlertsAsync(Guid userId)
    {
        var alerts = new List<AlertDto>();

        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .Include(c => c.Certifications)
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null) return alerts;

        // Profile completion alert
        var completionPercentage = CalculateProfileCompletion(candidate);
        if (completionPercentage < 100)
        {
            alerts.Add(new AlertDto
            {
                Title = "Profile Configuration",
                Description = $"Your profile is {completionPercentage}% complete. Complete it to increase your visibility.",
                Quantity = completionPercentage,
                Url = "/profile",
                AlertType = AlertType.Warning
            });
        }

        // No experience alert
        if (candidate.Experiences == null || !candidate.Experiences.Any(e => !e.IsDeleted))
        {
            alerts.Add(new AlertDto
            {
                Title = "No Work Experience",
                Description = "You haven't added any work experience yet. Add your first job to stand out.",
                Quantity = 0,
                Url = "/profile",
                AlertType = AlertType.Info
            });
        }

        // No education alert
        if (candidate.Educations == null || !candidate.Educations.Any(e => !e.IsDeleted))
        {
            alerts.Add(new AlertDto
            {
                Title = "No Education Records",
                Description = "Add your educational background to strengthen your profile.",
                Quantity = 0,
                Url = "/profile",
                AlertType = AlertType.Info
            });
        }

        // Profile not public alert
        if (!candidate.IsProfilePublic)
        {
            alerts.Add(new AlertDto
            {
                Title = "Profile is Private",
                Description = "Your profile is not visible to recruiters. Make it public to receive opportunities.",
                Quantity = null,
                Url = "/profile",
                AlertType = AlertType.Warning
            });
        }

        return alerts;
    }

    private static int CalculateProfileCompletion(Models.Entities.PTCandidate candidate)
    {
        var fields = new List<bool>
        {
            !string.IsNullOrWhiteSpace(candidate.FirstName),
            !string.IsNullOrWhiteSpace(candidate.LastName),
            !string.IsNullOrWhiteSpace(candidate.Title),
            !string.IsNullOrWhiteSpace(candidate.Summary),
            !string.IsNullOrWhiteSpace(candidate.Phone),
            !string.IsNullOrWhiteSpace(candidate.Country),
            !string.IsNullOrWhiteSpace(candidate.City),
            !string.IsNullOrWhiteSpace(candidate.LinkedInUrl),
            !string.IsNullOrWhiteSpace(candidate.PortfolioUrl),
            !string.IsNullOrWhiteSpace(candidate.CvUrl),
            candidate.YearsOfExperience.HasValue,
            candidate.Availability.HasValue,
            candidate.Experiences != null && candidate.Experiences.Any(e => !e.IsDeleted),
            candidate.Educations != null && candidate.Educations.Any(e => !e.IsDeleted),
            candidate.IsProfilePublic
        };

        var completed = fields.Count(f => f);
        return (int)Math.Round((double)completed / fields.Count * 100);
    }
}
