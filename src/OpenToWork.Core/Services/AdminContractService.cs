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

    public async Task<AdminVacancyContractDto?> GetByVacancyAsync(Guid vacancyId)
    {
        return await _context.PT_VacancyContracts
            .Where(c => c.PT_VacancyId == vacancyId && !c.IsDeleted)
            .Select(c => new AdminVacancyContractDto
            {
                Id = c.Id,
                VacancyId = c.PT_VacancyId,
                CompanyId = c.PT_CompanyId,
                CompanyName = c.Company.Name,
                CompanyContactName = c.Company.ContactName,
                CompanyContactPhone = c.Company.ContactPhone,
                VacancyTitle = c.Vacancy!.Title,
                ContractNumber = c.ContractNumber,
                Status = c.Status,
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

    public async Task<AdminVacancyContractDto?> SaveAsync(Guid vacancyId, AdminSaveVacancyContractDto dto, Guid adminId, string? ipAddress)
    {
        var vacancy = await _context.PT_Vacancies
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (vacancy == null) return null;

        // Validacion de distribucion de pago: debe sumar 100.
        if (dto.PaymentOpeningPct + dto.PaymentValidationPct + dto.PaymentConsolidationPct != 100m)
            return null;

        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.PT_VacancyId == vacancyId && !c.IsDeleted);

        // Solo el borrador es editable; un rechazado puede reabrirse como borrador.
        // Accepted/Cancelled/Sent quedan bloqueados.
        if (contract != null && contract.Status != (int)ContractStatus.Draft && contract.Status != (int)ContractStatus.Rejected)
            return null;

        if (contract != null && contract.Status == (int)ContractStatus.Rejected)
        {
            contract.Status = (int)ContractStatus.Draft;
            contract.RejectedAt = null;
            contract.RejectionReason = null;
        }

        if (contract == null)
        {
            contract = new PTVacancyContract
            {
                PT_VacancyId = vacancyId,
                PT_CompanyId = vacancy.PT_CompanyId,
                ContractNumber = await GenerateContractNumberAsync(),
                Status = (int)ContractStatus.Draft,
                CreatedBy = adminId
            };
            _context.PT_VacancyContracts.Add(contract);
        }

        contract.ScopeServices = dto.ScopeServices.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(dto.ScopeServices);
        contract.TargetCandidates = dto.TargetCandidates ?? vacancy.RequiredApplicants;
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

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, contract.CreatedAt == contract.UpdatedAt ? "CreateContract" : "UpdateContract",
            "PT_VacancyContracts", contract.Id, $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);

        return await GetByVacancyAsync(vacancyId);
    }

    /// <summary>Marca el anexo como enviado a la empresa (Draft -> Sent). Solo desde borrador.</summary>
    public async Task<bool> SendAsync(Guid vacancyId, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.PT_VacancyId == vacancyId && !c.IsDeleted);
        if (contract == null || contract.Status != (int)ContractStatus.Draft) return false;

        contract.Status = (int)ContractStatus.Sent;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SendContract", "PT_VacancyContracts", contract.Id,
            $"{{\"contractNumber\":\"{contract.ContractNumber}\"}}", ipAddress);
        return true;
    }

    public async Task<bool> DecideAsync(Guid vacancyId, bool accepted, string? reason, Guid adminId, string? ipAddress)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.PT_VacancyId == vacancyId && !c.IsDeleted);
        if (contract == null) return false;

        // Un anexo en borrador o enviado (pendiente de respuesta) puede aceptarse o rechazarse.
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
