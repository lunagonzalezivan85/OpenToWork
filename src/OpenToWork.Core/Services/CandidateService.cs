using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

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
