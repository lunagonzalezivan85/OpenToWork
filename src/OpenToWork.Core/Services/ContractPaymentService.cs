using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class ContractPaymentService : IContractPaymentService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public ContractPaymentService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task CreateTranchesForContractAsync(Guid contractId)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);
        if (contract == null) return;

        // Reaceptacion de una nueva version del contrato: los tramos ya existen.
        var alreadyExists = await _context.PT_ContractPayments
            .AnyAsync(p => p.PT_VacancyContractId == contractId && !p.IsDeleted);
        if (alreadyExists)
        {
            await RecalculateTranchesAsync(contract);
            return;
        }

        var feeAmount = contract.FeeAmount ?? 0m;
        var tranches = new[]
        {
            (Type: PaymentTrancheType.Apertura, Pct: contract.PaymentOpeningPct),
            (Type: PaymentTrancheType.Validacion, Pct: contract.PaymentValidationPct),
            (Type: PaymentTrancheType.Consolidacion, Pct: contract.PaymentConsolidationPct)
        };

        foreach (var (type, pct) in tranches)
        {
            _context.PT_ContractPayments.Add(new PTContractPayment
            {
                PT_VacancyContractId = contractId,
                TrancheType = (int)type,
                Percentage = pct,
                Amount = Math.Round(feeAmount * pct / 100m, 2),
                Status = (int)PaymentTrancheStatus.Pendiente
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>Ajusta los tramos 30/50/20 al importe y porcentajes de la version vigente (decision de
    /// Darwin, 24-Sep): lo ya cobrado no se toca; los tramos pendientes se recalculan; la diferencia
    /// sobre los tramos ya pagados va a un unico tramo "Ajuste" (positivo = falta cobrar, negativo =
    /// saldo a favor de la empresa), descontando ajustes ya pagados de versiones anteriores.</summary>
    private async Task RecalculateTranchesAsync(PTVacancyContract contract)
    {
        var payments = await _context.PT_ContractPayments
            .Where(p => p.PT_VacancyContractId == contract.Id && !p.IsDeleted)
            .ToListAsync();

        var feeAmount = contract.FeeAmount ?? 0m;
        var baseTranches = new[]
        {
            (Type: PaymentTrancheType.Apertura, Pct: contract.PaymentOpeningPct),
            (Type: PaymentTrancheType.Validacion, Pct: contract.PaymentValidationPct),
            (Type: PaymentTrancheType.Consolidacion, Pct: contract.PaymentConsolidationPct)
        };

        var owedOnPaid = 0m;
        foreach (var (type, pct) in baseTranches)
        {
            var target = Math.Round(feeAmount * pct / 100m, 2);
            var tranche = payments.FirstOrDefault(p => p.TrancheType == (int)type);
            if (tranche == null)
            {
                _context.PT_ContractPayments.Add(new PTContractPayment
                {
                    PT_VacancyContractId = contract.Id,
                    TrancheType = (int)type,
                    Percentage = pct,
                    Amount = target,
                    Status = (int)PaymentTrancheStatus.Pendiente
                });
            }
            else if (tranche.Status == (int)PaymentTrancheStatus.Pendiente)
            {
                tranche.Percentage = pct;
                tranche.Amount = target;
                tranche.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                owedOnPaid += target - tranche.Amount;
            }
        }

        var adjustments = payments.Where(p => p.TrancheType == (int)PaymentTrancheType.Ajuste).ToList();
        var alreadySettled = adjustments.Where(p => p.Status == (int)PaymentTrancheStatus.Pagado).Sum(p => p.Amount);
        foreach (var pending in adjustments.Where(p => p.Status == (int)PaymentTrancheStatus.Pendiente))
        {
            pending.IsDeleted = true;
            pending.DeletedAt = DateTime.UtcNow;
        }

        var net = Math.Round(owedOnPaid - alreadySettled, 2);
        if (net != 0m)
        {
            _context.PT_ContractPayments.Add(new PTContractPayment
            {
                PT_VacancyContractId = contract.Id,
                TrancheType = (int)PaymentTrancheType.Ajuste,
                Percentage = 0,
                Amount = net,
                Status = (int)PaymentTrancheStatus.Pendiente,
                Notes = net > 0
                    ? $"Version {contract.Version}: diferencia a cobrar sobre tramos ya pagados."
                    : $"Version {contract.Version}: saldo a favor de la empresa sobre tramos ya pagados."
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ContractPaymentDto>> GetByContractAsync(Guid contractId)
    {
        return await _context.PT_ContractPayments
            .Where(p => p.PT_VacancyContractId == contractId && !p.IsDeleted)
            .OrderBy(p => p.TrancheType)
            .Select(p => new ContractPaymentDto
            {
                Id = p.Id,
                ContractId = p.PT_VacancyContractId,
                TrancheType = p.TrancheType,
                Percentage = p.Percentage,
                Amount = p.Amount,
                Status = p.Status,
                PaidAt = p.PaidAt,
                PaidByName = p.PaidByUser != null ? p.PaidByUser.Email : null,
                Notes = p.Notes
            })
            .ToListAsync();
    }

    public async Task<ContractPaymentDto?> MarkAsPaidAsync(Guid trancheId, MarkTranchePaidDto dto, Guid adminId)
    {
        var tranche = await _context.PT_ContractPayments
            .FirstOrDefaultAsync(p => p.Id == trancheId && !p.IsDeleted);
        if (tranche == null) return null;

        tranche.Status = (int)PaymentTrancheStatus.Pagado;
        tranche.PaidAt = DateTime.UtcNow;
        tranche.PaidByUserId = adminId;
        tranche.Notes = dto.Notes;
        tranche.UpdatedAt = DateTime.UtcNow;
        tranche.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "MarkTranchePaid", "PT_ContractPayments", tranche.Id,
            $"{{\"trancheType\":{tranche.TrancheType},\"amount\":{tranche.Amount}}}", null);

        var result = await GetByContractAsync(tranche.PT_VacancyContractId);
        return result.FirstOrDefault(p => p.Id == trancheId);
    }

    public async Task<bool> IsOpeningPaidForVacancyAsync(Guid vacancyId)
    {
        // PT_ContractVacancies tiene indice unico (PT_VacancyId, IsDeleted): una vacante activa
        // solo puede estar ligada a un contrato a la vez.
        var contractId = await _context.PT_ContractVacancies
            .Where(cv => cv.PT_VacancyId == vacancyId && !cv.IsDeleted)
            .Select(cv => (Guid?)cv.PT_ContractId)
            .FirstOrDefaultAsync();

        if (contractId == null) return true; // vacante sin contrato asociado: no se bloquea

        var openingTranche = await _context.PT_ContractPayments
            .FirstOrDefaultAsync(p => p.PT_VacancyContractId == contractId.Value
                && p.TrancheType == (int)PaymentTrancheType.Apertura
                && !p.IsDeleted);

        // Si el tramo todavia no existe (el contrato aun no fue aceptado), se considera no pagado.
        return openingTranche != null && openingTranche.Status == (int)PaymentTrancheStatus.Pagado;
    }

    public async Task<Guid> CreateReplacementChargeAsync(Guid contractId, decimal percentage, string description)
    {
        var contract = await _context.PT_VacancyContracts
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        var feeAmount = contract?.FeeAmount ?? 0m;
        var tranche = new PTContractPayment
        {
            PT_VacancyContractId = contractId,
            TrancheType = (int)PaymentTrancheType.ReposicionSegunda,
            Percentage = percentage,
            Amount = Math.Round(feeAmount * percentage / 100m, 2),
            Status = (int)PaymentTrancheStatus.Pendiente,
            Notes = description
        };

        _context.PT_ContractPayments.Add(tranche);
        await _context.SaveChangesAsync();

        return tranche.Id;
    }
}
