using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminContractService : IAdminContractService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public AdminContractService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<AdminVacancyContractDto?> GetByIdAsync(Guid contractId)
    {
        return await ProjectContractAsync(c => c.Id == contractId && !c.IsDeleted);
    }

    public async Task<AdminVacancyContractDto?> GetByCompanyAsync(Guid companyId)
    {
        return await ProjectContractAsync(c => c.PT_CompanyId == companyId && !c.IsDeleted);
    }

    private async Task<AdminVacancyContractDto?> ProjectContractAsync(System.Linq.Expressions.Expression<Func<PTVacancyContract, bool>> predicate)
    {
        return await _context.PT_VacancyContracts
            .Where(predicate)
            .Select(c => new AdminVacancyContractDto
            {
                Id = c.Id,
                CompanyId = c.PT_CompanyId,
                CompanyName = c.Company.Name,
                CompanyLegalName = c.Company.LegalName,
                CompanyTaxId = c.Company.TaxId,
                CompanyAddress = c.Company.Address,
                CompanyCountry = c.Company.Country,
                CompanyCity = c.Company.City,
                CompanyContactName = c.Company.ContactName,
                CompanyContactPosition = c.Company.ContactPosition,
                CompanyContactDniNie = c.Company.ContactDniNie,
                CompanyContactEmail = c.Company.ContactEmail,
                CompanyContactPhone = c.Company.ContactPhone,
                ContractNumber = c.ContractNumber,
                Status = c.Status,
                Vacancies = c.ContractVacancies
                    .Where(cv => !cv.IsDeleted)
                    .Select(cv => new ContractVacancyItemDto
                    {
                        VacancyId = cv.PT_VacancyId,
                        Title = cv.Vacancy!.Title,
                        Description = cv.Vacancy.Description,
                        Requirements = cv.Vacancy.Requirements,
                        Location = cv.Vacancy.Location,
                        Category = cv.Vacancy.Category,
                        ContractType = cv.Vacancy.ContractType,
                        WorkMode = cv.Vacancy.WorkMode,
                        SalaryMin = cv.Vacancy.SalaryMin,
                        SalaryMax = cv.Vacancy.SalaryMax,
                        RequiredApplicants = cv.Vacancy.RequiredApplicants,
                        YearsExperience = cv.Vacancy.YearsExperience
                    }).ToList(),
                ScopeServices = c.ScopeServices == null
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.ScopeServices) ?? new List<string>(),
                TargetCandidates = c.TargetCandidates,
                JobTypeCategory = c.JobTypeCategory,
                TargetCoverageDays = c.TargetCoverageDays,
                WarrantyDays = c.WarrantyDays,
                FeeAmount = c.FeeAmount,
                Currency = c.Currency,
                FeeApplicationType = c.FeeApplicationType,
                PaymentOpeningPct = c.PaymentOpeningPct,
                PaymentValidationPct = c.PaymentValidationPct,
                PaymentConsolidationPct = c.PaymentConsolidationPct,
                FeeExceptions = c.FeeExceptions,
                AcceptedAt = c.AcceptedAt,
                RejectedAt = c.RejectedAt,
                RejectionReason = c.RejectionReason,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AdminVacancyContractDto?> CreateAsync(Guid companyId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress)
    {
        if (dto.PaymentOpeningPct + dto.PaymentValidationPct + dto.PaymentConsolidationPct != 100m)
            return null;

        var company = await _context.PT_Companies.FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);
        if (company == null) return null;

        var contract = new PTVacancyContract
        {
            PT_CompanyId = companyId,
            ContractNumber = await GenerateContractNumberAsync(),
            Status = (int)ContractStatus.Draft,
            CreatedBy = adminId
        };
        ApplyDtoToContract(contract, dto, adminId);

        foreach (var vacancyId in dto.VacancyIds.Distinct())
        {
            contract.ContractVacancies.Add(new PTContractVacancy
            {
                PT_VacancyId = vacancyId,
                CreatedBy = adminId
            });
        }

        _context.PT_VacancyContracts.Add(contract);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CreateContract",
            "PT_VacancyContracts", contract.Id, $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        return await GetByIdAsync(contract.Id);
    }

    public async Task<AdminVacancyContractDto?> SaveAsync(Guid contractId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress)
    {
        if (dto.PaymentOpeningPct + dto.PaymentValidationPct + dto.PaymentConsolidationPct != 100m)
            return null;

        var contract = await _context.PT_VacancyContracts
            .Include(c => c.ContractVacancies)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        if (contract == null) return null;
        if (contract.Status != (int)ContractStatus.Draft && contract.Status != (int)ContractStatus.Rejected)
            return null;

        if (contract.Status == (int)ContractStatus.Rejected)
        {
            contract.Status = (int)ContractStatus.Draft;
            contract.RejectedAt = null;
            contract.RejectionReason = null;
        }

        ApplyDtoToContract(contract, dto, adminId);

        // Sync vacancy links: remove deleted, add new
        var existingVacancyIds = contract.ContractVacancies.Where(cv => !cv.IsDeleted).Select(cv => cv.PT_VacancyId).ToHashSet();
        var newVacancyIds = dto.VacancyIds.Distinct().ToHashSet();

        foreach (var cv in contract.ContractVacancies.Where(cv => !cv.IsDeleted && !newVacancyIds.Contains(cv.PT_VacancyId)))
        {
            cv.IsDeleted = true;
            cv.DeletedAt = DateTime.UtcNow;
            cv.DeletedBy = adminId;
        }

        foreach (var vacancyId in newVacancyIds.Except(existingVacancyIds))
        {
            contract.ContractVacancies.Add(new PTContractVacancy
            {
                PT_VacancyId = vacancyId,
                CreatedBy = adminId
            });
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateContract",
            "PT_VacancyContracts", contract.Id, $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        return await GetByIdAsync(contractId);
    }

    private void ApplyDtoToContract(PTVacancyContract contract, AdminSaveVacancyContractDto dto, Guid adminId)
    {
        contract.ScopeServices = dto.ScopeServices.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(dto.ScopeServices);
        contract.TargetCandidates = dto.TargetCandidates;
        contract.JobTypeCategory = dto.JobTypeCategory;
        contract.TargetCoverageDays = dto.TargetCoverageDays;
        contract.WarrantyDays = dto.WarrantyDays;
        contract.FeeAmount = dto.FeeAmount;
        contract.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EUR" : dto.Currency.Trim().ToUpperInvariant();
        contract.FeeApplicationType = dto.FeeApplicationType;
        contract.PaymentOpeningPct = dto.PaymentOpeningPct;
        contract.PaymentValidationPct = dto.PaymentValidationPct;
        contract.PaymentConsolidationPct = dto.PaymentConsolidationPct;
        contract.FeeExceptions = dto.FeeExceptions;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;
    }

    /// <summary>Marca el anexo como enviado a la empresa (Draft -> Sent). Solo desde borrador.</summary>
    public async Task<bool> SendAsync(Guid contractId, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);
        if (contract == null || contract.Status != (int)ContractStatus.Draft) return false;

        contract.Status = (int)ContractStatus.Sent;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SendContract", "PT_VacancyContracts", contract.Id,
            $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);
        return true;
    }

    public async Task<bool> DecideAsync(Guid contractId, bool accepted, string? reason, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);
        if (contract == null) return false;

        if (contract.Status != (int)ContractStatus.Sent && contract.Status != (int)ContractStatus.Draft) return false;

        if (accepted)
        {
            contract.Status = (int)ContractStatus.Accepted;
            contract.AcceptedAt = DateTime.UtcNow;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(reason)) return false;
            contract.Status = (int)ContractStatus.Rejected;
            contract.RejectedAt = DateTime.UtcNow;
            contract.RejectionReason = reason.Trim();
        }
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, accepted ? "AcceptContract" : "RejectContract",
            "PT_VacancyContracts", contract.Id,
            $"{{\"contractNumber\":\"{contract.ContractNumber}\",\"accepted\":{accepted.ToString().ToLower()}}}", ipAddress);
        return true;
    }

    private async Task<string> GenerateContractNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"TD-{year}-";
        var last = await _context.PT_VacancyContracts
            .Where(c => c.ContractNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ContractNumber)
            .Select(c => c.ContractNumber)
            .FirstOrDefaultAsync();

        var next = 1;
        if (last != null && int.TryParse(last[prefix.Length..], out var seq)) next = seq + 1;
        return $"{prefix}{next:D4}";
    }
}
