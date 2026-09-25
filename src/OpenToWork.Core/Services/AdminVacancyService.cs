using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminVacancyService : IAdminVacancyService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public AdminVacancyService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<AdminVacancyResultDto> GetVacanciesAsync(int page, int pageSize, int? status, Guid? companyId = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1_000_000);

        var permanentQuery = _context.PT_Vacancies
            .Where(v => !v.IsDeleted && (companyId == null || v.PT_CompanyId == companyId.Value))
            .Select(v => new AdminVacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                CompanyName = v.Company.Name,
                Location = v.Location,
                ContractType = v.ContractType,
                WorkMode = v.WorkMode,
                Status = v.Status,
                IsTemporary = false,
                PublishedAt = v.PublishedAt,
                ClosedAt = v.ClosedAt,
                ExpiresAt = null,
                ViewsCount = v.ViewsCount
            });

        var tempQuery = _context.PT_TempVacancies
            .Where(v => !v.IsDeleted && companyId == null)
            .Select(v => new AdminVacancyDto
            {
                Id = v.Id,
                Title = v.Title,
                CompanyName = null,
                Location = v.Location,
                ContractType = v.ContractType,
                WorkMode = v.WorkMode,
                Status = v.IsPublished ? (int)VacancyStatus.Active : (int)VacancyStatus.Draft,
                IsTemporary = true,
                PublishedAt = null,
                ClosedAt = null,
                ExpiresAt = v.ExpiresAt,
                ViewsCount = 0
            });

        // Both sides assign the exact same set of AdminVacancyDto properties (with explicit
        // nulls for the ones that don't apply) so EF/Pomelo can translate this Concat into a
        // single UNION ALL query with server-side ORDER BY/LIMIT/OFFSET instead of loading
        // both tables into memory.
        var combined = permanentQuery.Concat(tempQuery);

        if (status.HasValue) combined = combined.Where(v => v.Status == status.Value);

        var totalCount = await combined.CountAsync();

        var items = await combined
            .OrderByDescending(v => v.PublishedAt ?? v.ExpiresAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        await SetDaysSinceContractSignedAsync(items);

        return new AdminVacancyResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>Dias desde la firma del contrato hasta el cierre de la vacante (o hasta hoy), pedido de
    /// Darwin 25-Sep. La firma es la PRIMERA aceptacion (audit log "AcceptContract"): abrir una nueva
    /// version del contrato y reaceptarla no reinicia la cuenta.</summary>
    private async Task SetDaysSinceContractSignedAsync(List<AdminVacancyDto> items)
    {
        var vacancyIds = items.Where(i => !i.IsTemporary).Select(i => i.Id).ToList();
        if (vacancyIds.Count == 0) return;

        var contracts = await _context.PT_ContractVacancies
            .Where(cv => vacancyIds.Contains(cv.PT_VacancyId) && !cv.IsDeleted && !cv.Contract.IsDeleted)
            .Select(cv => new { cv.PT_VacancyId, cv.PT_ContractId, cv.Contract.AcceptedAt, cv.Contract.TargetCoverageDays })
            .ToListAsync();
        if (contracts.Count == 0) return;

        var contractIds = contracts.Select(c => c.PT_ContractId).Distinct().ToList();
        var firstAcceptance = await _context.AD_AuditLogs
            .Where(a => a.Action == "AcceptContract" && a.EntityId != null && contractIds.Contains(a.EntityId.Value))
            .GroupBy(a => a.EntityId!.Value)
            .Select(g => new { ContractId = g.Key, At = g.Min(a => a.CreatedAt) })
            .ToDictionaryAsync(x => x.ContractId, x => x.At);

        foreach (var item in items)
        {
            var contract = contracts.FirstOrDefault(c => c.PT_VacancyId == item.Id);
            if (contract == null) continue;

            DateTime? signedAt = firstAcceptance.TryGetValue(contract.PT_ContractId, out var first) ? first : contract.AcceptedAt;
            if (signedAt == null) continue;

            var end = item.Status == (int)VacancyStatus.Closed && item.ClosedAt.HasValue ? item.ClosedAt.Value : DateTime.UtcNow;
            item.ContractSignedAt = signedAt;
            // Fechas en hora local (la misma con la que el admin las muestra), no UTC: si no, cerca de la
            // medianoche el conteo sale corrido un dia respecto de las fechas visibles.
            var from = DateTime.SpecifyKind(signedAt.Value, DateTimeKind.Utc).ToLocalTime().Date;
            var to = DateTime.SpecifyKind(end, DateTimeKind.Utc).ToLocalTime().Date;
            item.DaysSinceSigned = Math.Max(0, (int)(to - from).TotalDays);
            item.BusinessDaysSinceSigned = BusinessDaysBetween(from, to);
            item.TargetCoverageDays = contract.TargetCoverageDays;
        }
    }

    /// <summary>Dias habiles (lunes a viernes) transcurridos despues de 'from' hasta 'to' inclusive.</summary>
    private static int BusinessDaysBetween(DateTime from, DateTime to)
    {
        var days = 0;
        for (var d = from.AddDays(1); d <= to; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) days++;
        return days;
    }

    /// <summary>Codigo de PT_Vacancy.Category (texto libre historico) -> nombre del PTJobType
    /// sembrado (docs/seed-job-pricing.sql). Puente temporal mientras el formulario de
    /// vacante no seleccione el tipo de puesto directamente por Id.</summary>
    internal static readonly Dictionary<string, string> CategoryToJobTypeName = new()
    {
        ["Camarero"] = "Camarero/a",
        ["AyudanteCamarero"] = "Ayudante de camarero/a",
        ["AyudanteCocina"] = "Ayudante de cocina",
        ["AyudanteBarra"] = "Ayudante de barra",
        ["Cocinero"] = "Cocinero/a",
        ["Barman"] = "Barman / Bartender",
        ["RecepcionistaHotel"] = "Recepcionista de hotel",
        ["JefeSala"] = "Jefe/a de sala",
        ["ResponsableLocal"] = "Responsable de local"
    };

    /// <summary>Inverso de CategoryToJobTypeName: nombre del PTJobType -> codigo legado de Category,
    /// para que las vacantes creadas seleccionando el tipo de puesto por Id sigan siendo
    /// encontradas por los filtros de busqueda publicos que todavia usan el codigo.</summary>
    internal static readonly Dictionary<string, string> JobTypeNameToCategory =
        CategoryToJobTypeName.ToDictionary(kv => kv.Value, kv => kv.Key);

    private async Task<Guid?> ResolveJobTypeIdAsync(string? category)
    {
        if (string.IsNullOrEmpty(category) || !CategoryToJobTypeName.TryGetValue(category, out var name))
            return null;
        return await _context.PT_JobTypes
            .Where(t => t.Name == name && !t.IsDeleted)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<AdminVacancyDto?> CreateAsync(AdminCreateVacancyDto dto, Guid adminId, string? ipAddress)
    {
        var companyExists = await _context.PT_Companies.AnyAsync(c => c.Id == dto.CompanyId && !c.IsDeleted);
        if (!companyExists) return null;

        var category = dto.Category;
        var jobTypeId = dto.JobTypeId;
        if (jobTypeId.HasValue)
        {
            var jobType = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == jobTypeId.Value && !t.IsDeleted);
            if (jobType != null)
            {
                // Los filtros de busqueda publicos (Vacancies.razor/Home.razor) comparan Category
                // contra el codigo legado (ej. "AyudanteBarra"), no el nombre visible.
                category = JobTypeNameToCategory.TryGetValue(jobType.Name, out var legacyCode)
                    ? legacyCode
                    : jobType.Name;
            }
        }
        else
        {
            jobTypeId = await ResolveJobTypeIdAsync(dto.Category);
        }

        var vacancy = new PTVacancy
        {
            Id = Guid.NewGuid(),
            PT_CompanyId = dto.CompanyId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Requirements = dto.Requirements,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            Location = dto.Location,
            ContractType = dto.ContractType,
            WorkMode = dto.WorkMode,
            Category = category,
            PT_JobTypeId = jobTypeId,
            ExperienceLevel = dto.ExperienceLevel,
            EnglishLevel = dto.EnglishLevel,
            RequiredApplicants = dto.RequiredApplicants,
            YearsExperience = dto.YearsExperience,
            Status = (int)VacancyStatus.Active,
            PublishedAt = DateTime.UtcNow,
            CreatedBy = adminId
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
                    CreatedBy = adminId
                });
            }
            if (validIds.Count > 0) await _context.SaveChangesAsync();
        }

        await _auditLog.LogAsync(adminId, "CreateVacancy", "PT_Vacancies", vacancy.Id, $"{{\"title\":\"{dto.Title}\"}}", ipAddress);

        return new AdminVacancyDto
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            CompanyName = (await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == dto.CompanyId))?.Name,
            Location = vacancy.Location,
            ContractType = vacancy.ContractType,
            WorkMode = vacancy.WorkMode,
            Status = vacancy.Status,
            IsTemporary = false,
            PublishedAt = vacancy.PublishedAt,
            ViewsCount = 0
        };
    }

    public async Task<VacancyDto?> GetByIdAsync(Guid id)
    {
        return await _context.PT_Vacancies
            .Where(v => v.Id == id && !v.IsDeleted)
            .Select(v => new VacancyDto
            {
                Id = v.Id,
                CompanyId = v.PT_CompanyId,
                CompanyName = v.Company.Name,
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
                RequiredApplicants = v.RequiredApplicants,
                YearsExperience = v.YearsExperience,
                Status = v.Status,
                PublishedAt = v.PublishedAt,
                ViewsCount = v.ViewsCount,
                JobTypeId = v.PT_JobTypeId,
                JobTypeName = v.JobType != null ? v.JobType.Name : null,
                JobLevelName = v.JobType != null ? v.JobType.JobLevel.Name : null,
                Skills = v.VacancySkills
                    .Where(vs => !vs.IsDeleted)
                    .OrderBy(vs => vs.Skill.Name)
                    .Select(vs => vs.Skill.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CreateApplicationAsync(Guid vacancyId, AdminCreateApplicationDto dto, Guid adminId, string? ipAddress)
    {
        var vacancyExists = await _context.PT_Vacancies.AnyAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (!vacancyExists) return false;

        var candidateExists = await _context.PT_Candidates.AnyAsync(c => c.Id == dto.CandidateId && !c.IsDeleted);
        if (!candidateExists) return false;

        var alreadyApplied = await _context.PT_Applications
            .AnyAsync(a => a.PT_VacancyId == vacancyId && a.PT_CandidateId == dto.CandidateId && !a.IsDeleted);
        if (alreadyApplied) return false;

        var application = new PTApplication
        {
            PT_CandidateId = dto.CandidateId,
            PT_VacancyId = vacancyId,
            Status = (int)ApplicationStatus.Pending,
            ApplicationSource = (int)ApplicationSource.AdminCurated,
            CreatedBy = adminId
        };

        _context.PT_Applications.Add(application);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "CreateApplication", "PT_Applications", application.Id,
            $"{{\"candidateId\":\"{dto.CandidateId}\",\"vacancyId\":\"{vacancyId}\",\"source\":\"AdminCurated\"}}", ipAddress);
        return true;
    }

    public async Task<bool> ModerateAsync(Guid id, int status, Guid adminId, string? ipAddress)
    {
        var vacancy = await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (vacancy != null)
        {
            vacancy.Status = status;
            vacancy.UpdatedAt = DateTime.UtcNow;
            vacancy.UpdatedBy = adminId;
            if (status == (int)VacancyStatus.Closed) vacancy.ClosedAt = DateTime.UtcNow;
            if (status == (int)VacancyStatus.Active && vacancy.PublishedAt == null) vacancy.PublishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(adminId, "ModerateVacancy", "PT_Vacancies", id, $"{{\"status\":{status}}}", ipAddress);
            return true;
        }

        var tempVacancy = await _context.PT_TempVacancies.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (tempVacancy != null)
        {
            // PT_TempVacancy only has an IsPublished flag, with no field to represent "Closed" the way
            // PT_Vacancy.Status does. Treat Closed as terminal: unpublish and soft-delete so it stops
            // appearing (rather than falling back to a state indistinguishable from "never reviewed").
            tempVacancy.IsPublished = status == (int)VacancyStatus.Active;
            tempVacancy.UpdatedAt = DateTime.UtcNow;
            tempVacancy.UpdatedBy = adminId;
            if (status == (int)VacancyStatus.Closed)
            {
                tempVacancy.IsDeleted = true;
                tempVacancy.DeletedAt = DateTime.UtcNow;
                tempVacancy.DeletedBy = adminId;
            }

            await _context.SaveChangesAsync();
            await _auditLog.LogAsync(adminId, "ModerateVacancy", "PT_TempVacancies", id, $"{{\"status\":{status}}}", ipAddress);
            return true;
        }

        return false;
    }
}
