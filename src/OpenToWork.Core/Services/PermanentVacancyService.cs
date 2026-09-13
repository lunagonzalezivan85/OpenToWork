using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class PermanentVacancyService : IPermanentVacancyService
{
    private readonly AppDbContext _context;
    private readonly IContractPaymentService _payments;

    public PermanentVacancyService(AppDbContext context, IContractPaymentService payments)
    {
        _context = context;
        _payments = payments;
    }

    public async Task<VacancyDto> CreateVacancyAsync(Guid companyId, CreateVacancyDto dto, Guid userId)
    {
        var vacancy = new PTVacancy
        {
            PT_CompanyId = companyId,
            Title = dto.Title,
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
            Status = 0,
            CreatedBy = userId
        };

        _context.PT_Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
        return await MapToDtoAsync(vacancy);
    }

    public async Task<VacancyDto?> GetVacancyByIdAsync(Guid id)
    {
        var vacancy = await _context.PT_Vacancies
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vacancy == null) return null;

        vacancy.ViewsCount++;
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(vacancy);
    }

    public async Task<IEnumerable<VacancyDto>> GetVacanciesByCompanyAsync(Guid companyId)
    {
        var vacancies = await _context.PT_Vacancies
            .Include(v => v.Company)
            .Where(v => v.PT_CompanyId == companyId && !v.IsDeleted)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        var dtos = new List<VacancyDto>();
        foreach (var v in vacancies)
            dtos.Add(await MapToDtoAsync(v));
        return dtos;
    }

    public async Task<(IEnumerable<VacancyDto> Items, int Total)> SearchVacanciesAsync(SearchPermanentVacancyDto search)
    {
        var query = _context.PT_Vacancies
            .Include(v => v.Company)
            .Where(v => !v.IsDeleted && v.Status == 1);

        if (!string.IsNullOrEmpty(search.Query))
        {
            var q = search.Query.ToLower();
            query = query.Where(v => v.Title.ToLower().Contains(q) ||
                                     (v.Description != null && v.Description.ToLower().Contains(q)) ||
                                     (v.Requirements != null && v.Requirements.ToLower().Contains(q)));
        }

        if (!string.IsNullOrEmpty(search.Location))
            query = query.Where(v => v.Location != null && v.Location.Contains(search.Location));

        if (search.ContractType.HasValue)
            query = query.Where(v => v.ContractType == search.ContractType.Value);

        if (search.WorkMode.HasValue)
            query = query.Where(v => v.WorkMode == search.WorkMode.Value);

        if (!string.IsNullOrEmpty(search.Category))
            query = query.Where(v => v.Category != null && v.Category.Contains(search.Category));

        if (search.ExperienceLevel.HasValue)
            query = query.Where(v => v.ExperienceLevel == search.ExperienceLevel.Value);

        if (search.EnglishLevel.HasValue)
            query = query.Where(v => v.EnglishLevel >= search.EnglishLevel.Value);

        if (search.SalaryMin.HasValue)
            query = query.Where(v => v.SalaryMin >= search.SalaryMin.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(v => v.PublishedAt ?? v.CreatedAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();

        var dtos = new List<VacancyDto>();
        foreach (var v in items)
            dtos.Add(await MapToDtoAsync(v));
        return (dtos, total);
    }

    public async Task<VacancyDto?> UpdateVacancyAsync(Guid id, UpdateVacancyDto dto, Guid userId)
    {
        var vacancy = await _context.PT_Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vacancy == null) return null;

        if (dto.Title != null) vacancy.Title = dto.Title;
        if (dto.Description != null) vacancy.Description = dto.Description;
        if (dto.Requirements != null) vacancy.Requirements = dto.Requirements;
        if (dto.SalaryMin.HasValue) vacancy.SalaryMin = dto.SalaryMin;
        if (dto.SalaryMax.HasValue) vacancy.SalaryMax = dto.SalaryMax;
        if (dto.Location != null) vacancy.Location = dto.Location;
        if (dto.ContractType.HasValue) vacancy.ContractType = dto.ContractType.Value;
        if (dto.WorkMode.HasValue) vacancy.WorkMode = dto.WorkMode.Value;
        if (dto.Category != null) vacancy.Category = dto.Category;
        if (dto.ExperienceLevel.HasValue) vacancy.ExperienceLevel = dto.ExperienceLevel;
        if (dto.EnglishLevel.HasValue) vacancy.EnglishLevel = dto.EnglishLevel;
        if (dto.Status.HasValue) vacancy.Status = dto.Status.Value;
        vacancy.UpdatedAt = DateTime.UtcNow;
        vacancy.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(vacancy);
    }

    public async Task<bool> DeleteVacancyAsync(Guid id, Guid userId)
    {
        var vacancy = await _context.PT_Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vacancy == null) return false;

        vacancy.IsDeleted = true;
        vacancy.DeletedAt = DateTime.UtcNow;
        vacancy.DeletedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishVacancyAsync(Guid id, Guid userId)
    {
        var vacancy = await _context.PT_Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vacancy == null || vacancy.Status != 0) return false;

        if (!await _payments.IsOpeningPaidForVacancyAsync(id))
            throw new InvalidOperationException("Esta vacante pertenece a un contrato cuyo pago de apertura (30%) todavia no fue confirmado. El proceso no se activa hasta que Trato Directo confirme el cobro.");

        vacancy.Status = 1;
        vacancy.PublishedAt = DateTime.UtcNow;
        vacancy.UpdatedAt = DateTime.UtcNow;
        vacancy.UpdatedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseVacancyAsync(Guid id, Guid userId)
    {
        var vacancy = await _context.PT_Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vacancy == null || vacancy.Status != 1) return false;

        vacancy.Status = 2;
        vacancy.ClosedAt = DateTime.UtcNow;
        vacancy.UpdatedAt = DateTime.UtcNow;
        vacancy.UpdatedBy = userId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConvertTempVacancyAsync(Guid tempVacancyId, Guid userId)
    {
        var tempVacancy = await _context.PT_TempVacancies
            .FirstOrDefaultAsync(v => v.Id == tempVacancyId && !v.IsDeleted);

        if (tempVacancy == null) return false;

        var company = await _context.PT_Companies
            .FirstOrDefaultAsync(c => c.SCUserId == userId && !c.IsDeleted);

        if (company == null) return false;

        var vacancy = new PTVacancy
        {
            PT_CompanyId = company.Id,
            Title = tempVacancy.Title,
            Description = tempVacancy.Description,
            Requirements = tempVacancy.Requirements,
            SalaryMin = tempVacancy.SalaryMin,
            SalaryMax = tempVacancy.SalaryMax,
            Location = tempVacancy.Location,
            ContractType = tempVacancy.ContractType,
            WorkMode = tempVacancy.WorkMode,
            Category = tempVacancy.Category,
            ExperienceLevel = tempVacancy.ExperienceLevel,
            EnglishLevel = tempVacancy.EnglishLevel,
            Status = 1,
            PublishedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.PT_Vacancies.Add(vacancy);

        tempVacancy.IsDeleted = true;
        tempVacancy.DeletedAt = DateTime.UtcNow;
        tempVacancy.DeletedBy = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<VacancyDto> MapToDtoAsync(PTVacancy v)
    {
        var company = v.Company ?? await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == v.PT_CompanyId);
        return new VacancyDto
        {
            Id = v.Id,
            CompanyId = v.PT_CompanyId,
            CompanyName = company?.Name ?? string.Empty,
            CompanyLogoUrl = company?.LogoUrl,
            CompanyIsVerified = company?.IsVerified ?? false,
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
            Status = v.Status,
            PublishedAt = v.PublishedAt,
            ViewsCount = v.ViewsCount
        };
    }
}
