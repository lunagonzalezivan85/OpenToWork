using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class CandidateService : ICandidateService
{
    private readonly AppDbContext _context;

    public CandidateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDto?> GetCandidateByUserIdAsync(Guid userId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        return candidate == null ? null : MapToDto(candidate);
    }

    public async Task<CandidateDto?> GetCandidateByIdAsync(Guid id)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.Experiences)
            .Include(c => c.Educations)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return candidate == null ? null : MapToDto(candidate);
    }

    public async Task<CandidateDto> CreateCandidateAsync(Guid userId, string createdBy)
    {
        var existing = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (existing != null) return MapToDto(existing);

        var candidate = new PTCandidate
        {
            SCUserId = userId,
            WizardStep = 0,
            WizardCompleted = false,
            CreatedBy = Guid.Parse(createdBy)
        };

        _context.PT_Candidates.Add(candidate);
        await _context.SaveChangesAsync();
        return MapToDto(candidate);
    }

    public async Task<CandidateDto> UpdateWizardStepAsync(Guid userId, UpdateCandidateWizardDto dto)
    {
        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (candidate == null)
        {
            candidate = new PTCandidate { SCUserId = userId, WizardStep = 0, WizardCompleted = false };
            _context.PT_Candidates.Add(candidate);
        }

        if (dto.FirstName != null) candidate.FirstName = dto.FirstName;
        if (dto.LastName != null) candidate.LastName = dto.LastName;
        if (dto.Identification != null) candidate.Identification = dto.Identification;
        if (dto.Phone != null) candidate.Phone = dto.Phone;
        if (dto.BirthDate.HasValue) candidate.BirthDate = dto.BirthDate;
        if (dto.Gender.HasValue) candidate.Gender = dto.Gender;
        if (dto.Title != null) candidate.Title = dto.Title;
        if (dto.Summary != null) candidate.Summary = dto.Summary;
        if (dto.Country != null) candidate.Country = dto.Country;
        if (dto.City != null) candidate.City = dto.City;
        if (dto.Address != null) candidate.Address = dto.Address;

        candidate.WizardStep = dto.WizardStep;
        candidate.WizardCompleted = dto.WizardCompleted;
        candidate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(candidate);
    }

    public async Task<bool> IsWizardCompleteAsync(Guid userId)
    {
        return await _context.PT_Candidates
            .AnyAsync(c => c.SCUserId == userId && c.WizardCompleted && !c.IsDeleted);
    }

    /// <summary>Proceso del propio candidato para el portal: etapa del reclutamiento, empresas a las que
    /// Trato Directo lo presento y plan. Nunca expone notas internas, reclutador ni motivo de rechazo.</summary>
    public async Task<CandidateProcessDto> GetMyProcessAsync(Guid userId)
    {
        var result = new CandidateProcessDto();

        var candidate = await _context.PT_Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);
        if (candidate != null)
        {
            result.PlanTier = candidate.PlanTier;
            result.PlanExpiresAt = candidate.PlanExpiresAt;
            result.PlanIsActive = candidate.PlanTier != CandidatePlanTier.Free && PlanCalculator.IsActive(candidate.PlanExpiresAt);
        }

        var recruitment = await _context.PT_CandidateRecruitments
            .AsNoTracking()
            .Include(r => r.Vacancy).ThenInclude(v => v!.Company)
            .Include(r => r.StageLogs)
            .FirstOrDefaultAsync(r => r.SCUserId == userId && !r.IsDeleted);
        if (recruitment != null)
        {
            result.HasRecruitment = true;
            result.CurrentStage = recruitment.CurrentStage;
            result.StageEnteredAt = recruitment.StageEnteredAt;
            result.VacancyTitle = recruitment.Vacancy?.Title;
            result.VacancyCompanyName = recruitment.Vacancy?.Company?.Name;
            result.StageReachedAt[(int)RecruitmentStage.Postulation] = recruitment.CreatedAt;
            foreach (var log in recruitment.StageLogs.Where(l => !l.IsDeleted).OrderBy(l => l.CreatedAt))
                result.StageReachedAt.TryAdd(log.ToStage, log.CreatedAt);
        }

        if (candidate != null)
        {
            result.Deliveries = await _context.PT_CandidateDeliveries
                .AsNoTracking()
                .Where(d => d.PT_CandidateId == candidate.Id && !d.IsDeleted)
                .OrderByDescending(d => d.DeliveredAt)
                .Select(d => new CandidateProcessDeliveryDto
                {
                    VacancyId = d.PT_VacancyId,
                    VacancyTitle = d.Vacancy.Title,
                    CompanyName = d.Company.Name,
                    Location = d.Vacancy.Location,
                    DeliveredAt = d.DeliveredAt,
                    Status = d.Status,
                    IncorporationDate = d.IncorporationDate,
                    PlacementEnded = d.PlacementEndedAt != null
                })
                .ToListAsync();
        }

        return result;
    }

    private static CandidateDto MapToDto(PTCandidate c) => new()
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
        LinkedInUrl = c.LinkedInUrl,
        YearsOfExperience = c.YearsOfExperience,
        WizardCompleted = c.WizardCompleted,
        WizardStep = c.WizardStep,
        Experiences = c.Experiences.Where(e => !e.IsDeleted).Select(e => new CandidateExperienceDto
        {
            Id = e.Id,
            CandidateId = e.PT_CandidateId,
            JobTitle = e.JobTitle,
            CompanyName = e.CompanyName,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            IsCurrentJob = e.IsCurrentJob,
            Location = e.Location
        }).ToList(),
        Educations = c.Educations.Where(e => !e.IsDeleted).Select(e => new CandidateEducationDto
        {
            Id = e.Id,
            CandidateId = e.PT_CandidateId,
            Institution = e.Institution,
            Degree = e.Degree,
            FieldOfStudy = e.FieldOfStudy,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            IsInProgress = e.IsInProgress
        }).ToList()
    };
}
