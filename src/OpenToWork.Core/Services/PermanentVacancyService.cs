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
        var category = dto.Category;
        if (dto.PT_JobTypeId.HasValue)
        {
            var jobTypeName = await _context.PT_JobTypes
                .Where(t => t.Id == dto.PT_JobTypeId.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
            if (jobTypeName != null)
            {
                // Los filtros de busqueda publicos (Vacancies.razor/Home.razor) todavia comparan
                // Category contra el codigo legado (ej. "AyudanteBarra"), no el nombre visible.
                category = AdminVacancyService.JobTypeNameToCategory.TryGetValue(jobTypeName, out var legacyCode)
                    ? legacyCode
                    : jobTypeName;
            }
        }

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
            Category = category,
            PT_JobTypeId = dto.PT_JobTypeId,
            ExperienceLevel = dto.ExperienceLevel,
            EnglishLevel = dto.EnglishLevel,
            Status = 0,
            CreatedBy = userId
        };

        _context.PT_Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();

        if (dto.SkillIds is { Count: > 0 })
        {
            var requestedIds = dto.SkillIds.Distinct().ToList();
            var validIds = await _context.PT_Skills
                .Where(s => requestedIds.Contains(s.Id) && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var skillId in validIds)
            {
                _context.PT_VacancySkills.Add(new PTVacancySkill
                {
                    PT_VacancyId = vacancy.Id,
                    PT_SkillId = skillId,
                    IsRequired = true,
                    CreatedBy = userId
                });
            }
            if (validIds.Count > 0) await _context.SaveChangesAsync();
        }

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

        return await MapManyToDtoAsync(vacancies);
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

        var dtos = await MapManyToDtoAsync(items);
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

        var jobType = v.PT_JobTypeId.HasValue
            ? await _context.PT_JobTypes
                .Where(t => t.Id == v.PT_JobTypeId.Value)
                .Select(t => new { t.Id, t.Name, LevelName = t.JobLevel.Name })
                .FirstOrDefaultAsync()
            : null;

        var skills = await _context.PT_VacancySkills
            .Where(vs => vs.PT_VacancyId == v.Id && !vs.IsDeleted)
            .OrderBy(vs => vs.Skill.Name)
            .Select(vs => vs.Skill.Name)
            .ToListAsync();

        return BuildDto(v, company, jobType?.Id, jobType?.Name, jobType?.LevelName, skills);
    }

    /// <summary>Version batch de MapToDtoAsync: 2 queries totales (tipos de puesto + skills) en vez
    /// de 2 por vacante, para listar/buscar sin generar N+1.</summary>
    private async Task<List<VacancyDto>> MapManyToDtoAsync(List<PTVacancy> vacancies)
    {
        var jobTypeIds = vacancies.Where(v => v.PT_JobTypeId.HasValue).Select(v => v.PT_JobTypeId!.Value).Distinct().ToList();
        var jobTypes = jobTypeIds.Count == 0
            ? new Dictionary<Guid, (string Name, string LevelName)>()
            : (await _context.PT_JobTypes
                .Where(t => jobTypeIds.Contains(t.Id))
                .Select(t => new { t.Id, t.Name, LevelName = t.JobLevel.Name })
                .ToListAsync())
                .ToDictionary(t => t.Id, t => (t.Name, t.LevelName));

        var vacancyIds = vacancies.Select(v => v.Id).ToList();
        var skillsByVacancy = (await _context.PT_VacancySkills
                .Where(vs => vacancyIds.Contains(vs.PT_VacancyId) && !vs.IsDeleted)
                .OrderBy(vs => vs.Skill.Name)
                .Select(vs => new { vs.PT_VacancyId, SkillName = vs.Skill.Name })
                .ToListAsync())
            .GroupBy(x => x.PT_VacancyId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.SkillName).ToList());

        var dtos = new List<VacancyDto>(vacancies.Count);
        foreach (var v in vacancies)
        {
            var company = v.Company;
            (Guid, string, string)? jobType = v.PT_JobTypeId.HasValue && jobTypes.TryGetValue(v.PT_JobTypeId.Value, out var jt)
                ? (v.PT_JobTypeId.Value, jt.Name, jt.LevelName)
                : null;
            var skills = skillsByVacancy.TryGetValue(v.Id, out var s) ? s : new List<string>();

            dtos.Add(BuildDto(v, company, jobType?.Item1, jobType?.Item2, jobType?.Item3, skills));
        }
        return dtos;
    }

    private static VacancyDto BuildDto(PTVacancy v, PTCompany? company, Guid? jobTypeId, string? jobTypeName, string? jobLevelName, List<string> skills) => new()
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
        ViewsCount = v.ViewsCount,
        JobTypeId = jobTypeId,
        JobTypeName = jobTypeName,
        JobLevelName = jobLevelName,
        Skills = skills
    };
}
