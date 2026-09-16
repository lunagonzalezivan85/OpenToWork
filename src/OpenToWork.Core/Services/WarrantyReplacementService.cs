using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class WarrantyReplacementService : IWarrantyReplacementService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly IContractPaymentService _payments;

    public WarrantyReplacementService(AppDbContext context, IAuditLogService auditLog, IContractPaymentService payments)
    {
        _context = context;
        _auditLog = auditLog;
        _payments = payments;
    }

    public async Task<WarrantyReplacementDto?> ActivateFromNegotiationAsync(Guid negotiationId, ActivateWarrantyReplacementDto dto, Guid staffId, string? ipAddress)
    {
        var negotiation = await _context.PT_Negotiations
            .FirstOrDefaultAsync(n => n.Id == negotiationId && !n.IsDeleted);
        if (negotiation == null) return null;
        if (negotiation.Status != (int)NegotiationStatus.Cerrada || negotiation.IncorporationDate == null)
            throw new InvalidOperationException("La negociacion debe estar cerrada y con fecha de incorporacion registrada para activar una reposicion de garantia.");

        var contractId = await ResolveContractIdAsync(negotiation.PT_VacancyId);
        if (contractId == null)
            throw new InvalidOperationException("La vacante no tiene un contrato vinculado.");

        return await CreateReplacementAsync(negotiation.PT_VacancyId, contractId.Value, negotiationId, null, dto, staffId, ipAddress);
    }

    public async Task<WarrantyReplacementDto?> ActivateFromDeliveryAsync(Guid deliveryId, ActivateWarrantyReplacementDto dto, Guid staffId, string? ipAddress)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.Status != (int)DeliveryStatus.Hired || delivery.IncorporationDate == null)
            throw new InvalidOperationException("La entrega debe estar en estado Contratado y con fecha de incorporacion registrada para activar una reposicion de garantia.");

        var contractId = await ResolveContractIdAsync(delivery.PT_VacancyId);
        if (contractId == null)
            throw new InvalidOperationException("La vacante no tiene un contrato vinculado.");

        return await CreateReplacementAsync(delivery.PT_VacancyId, contractId.Value, null, deliveryId, dto, staffId, ipAddress);
    }

    public async Task<WarrantyReplacementDto?> LinkReplacementAsync(Guid replacementId, LinkWarrantyReplacementDto dto, Guid staffId)
    {
        var replacement = await _context.PT_WarrantyReplacements
            .FirstOrDefaultAsync(w => w.Id == replacementId && !w.IsDeleted);
        if (replacement == null) return null;
        if (replacement.Status != (int)WarrantyReplacementStatus.EnCurso)
            throw new InvalidOperationException("Solo se puede vincular una reposicion en curso.");

        if (dto.NegotiationId.HasValue)
        {
            var negotiation = await _context.PT_Negotiations
                .FirstOrDefaultAsync(n => n.Id == dto.NegotiationId.Value && !n.IsDeleted);
            if (negotiation == null || negotiation.PT_VacancyId != replacement.PT_VacancyId || negotiation.Status != (int)NegotiationStatus.Cerrada)
                throw new InvalidOperationException("La negociacion elegida no es valida para esta reposicion.");
            if (negotiation.Id == replacement.OriginalNegotiationId)
                throw new InvalidOperationException("No se puede vincular la misma negociacion original.");
            replacement.ReplacementNegotiationId = negotiation.Id;
        }
        else if (dto.DeliveryId.HasValue)
        {
            var delivery = await _context.PT_CandidateDeliveries
                .FirstOrDefaultAsync(d => d.Id == dto.DeliveryId.Value && !d.IsDeleted);
            if (delivery == null || delivery.PT_VacancyId != replacement.PT_VacancyId || delivery.Status != (int)DeliveryStatus.Hired)
                throw new InvalidOperationException("La entrega elegida no es valida para esta reposicion.");
            if (delivery.Id == replacement.OriginalDeliveryId)
                throw new InvalidOperationException("No se puede vincular la misma entrega original.");
            replacement.ReplacementDeliveryId = delivery.Id;
        }
        else
        {
            throw new InvalidOperationException("Debe indicar una negociacion o entrega para vincular.");
        }

        replacement.Status = (int)WarrantyReplacementStatus.Completada;
        replacement.CompletedAt = DateTime.UtcNow;
        replacement.UpdatedAt = DateTime.UtcNow;
        replacement.UpdatedBy = staffId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "LinkWarrantyReplacement", "PT_WarrantyReplacements", replacement.Id,
            $"{{\"negotiationId\":\"{dto.NegotiationId}\",\"deliveryId\":\"{dto.DeliveryId}\"}}", null);

        return await ToDtoAsync(replacement.Id);
    }

    public async Task<WarrantyReplacementDto?> CancelAsync(Guid replacementId, Guid staffId)
    {
        var replacement = await _context.PT_WarrantyReplacements
            .FirstOrDefaultAsync(w => w.Id == replacementId && !w.IsDeleted);
        if (replacement == null) return null;
        if (replacement.Status != (int)WarrantyReplacementStatus.EnCurso)
            throw new InvalidOperationException("Solo se puede cancelar una reposicion en curso.");

        replacement.Status = (int)WarrantyReplacementStatus.Cancelada;
        replacement.UpdatedAt = DateTime.UtcNow;
        replacement.UpdatedBy = staffId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "CancelWarrantyReplacement", "PT_WarrantyReplacements", replacement.Id, null, null);

        return await ToDtoAsync(replacement.Id);
    }

    public async Task<List<WarrantyReplacementDto>> GetByContractAsync(Guid contractId)
    {
        var ids = await _context.PT_WarrantyReplacements
            .Where(w => w.PT_VacancyContractId == contractId && !w.IsDeleted)
            .OrderByDescending(w => w.RequestedAt)
            .Select(w => w.Id)
            .ToListAsync();

        var result = new List<WarrantyReplacementDto>();
        foreach (var id in ids)
        {
            var dto = await ToDtoAsync(id);
            if (dto != null) result.Add(dto);
        }
        return result;
    }

    private async Task<Guid?> ResolveContractIdAsync(Guid vacancyId)
    {
        return await _context.PT_ContractVacancies
            .Where(cv => cv.PT_VacancyId == vacancyId && !cv.IsDeleted)
            .Select(cv => (Guid?)cv.PT_ContractId)
            .FirstOrDefaultAsync();
    }

    private async Task<WarrantyReplacementDto?> CreateReplacementAsync(Guid vacancyId, Guid contractId, Guid? originalNegotiationId, Guid? originalDeliveryId, ActivateWarrantyReplacementDto dto, Guid staffId, string? ipAddress)
    {
        if (!Enum.IsDefined(typeof(WarrantyReplacementReason), dto.Reason))
            throw new InvalidOperationException("Motivo invalido.");

        var reason = (WarrantyReplacementReason)dto.Reason;
        var isExclusion = reason.IsExclusion();

        var replacement = new PTWarrantyReplacement
        {
            PT_VacancyContractId = contractId,
            PT_VacancyId = vacancyId,
            OriginalNegotiationId = originalNegotiationId,
            OriginalDeliveryId = originalDeliveryId,
            Reason = dto.Reason,
            Notes = dto.Notes,
            IsExclusion = isExclusion,
            RequestedByUserId = staffId,
            RequestedAt = DateTime.UtcNow,
            CreatedBy = staffId
        };

        if (isExclusion)
        {
            replacement.ReplacementNumber = 0;
            replacement.Status = (int)WarrantyReplacementStatus.ExcluidaDeGarantia;
        }
        else
        {
            var previousCount = await _context.PT_WarrantyReplacements
                .CountAsync(w => w.PT_VacancyId == vacancyId && !w.IsDeleted
                    && w.Status != (int)WarrantyReplacementStatus.Cancelada
                    && w.Status != (int)WarrantyReplacementStatus.ExcluidaDeGarantia);

            var replacementNumber = previousCount + 1;
            if (replacementNumber > 2)
                throw new InvalidOperationException("Esta vacante ya agoto las 2 reposiciones cubiertas por la garantia.");

            replacement.ReplacementNumber = replacementNumber;
            replacement.Status = (int)WarrantyReplacementStatus.EnCurso;

            var vacancy = await _context.PT_Vacancies.FirstOrDefaultAsync(v => v.Id == vacancyId && !v.IsDeleted);
            if (vacancy != null)
            {
                vacancy.Status = (int)VacancyStatus.Active;
                vacancy.ClosedAt = null;
                vacancy.UpdatedAt = DateTime.UtcNow;
                vacancy.UpdatedBy = staffId;
            }

            if (replacementNumber == 2)
            {
                var trancheId = await _payments.CreateReplacementChargeAsync(contractId, 50m, "Segunda reposicion de garantia");
                replacement.ChargeTrancheId = trancheId;
            }
        }

        _context.PT_WarrantyReplacements.Add(replacement);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "ActivateWarrantyReplacement", "PT_WarrantyReplacements", replacement.Id,
            $"{{\"vacancyId\":\"{vacancyId}\",\"reason\":{dto.Reason},\"replacementNumber\":{replacement.ReplacementNumber}}}", ipAddress);

        return await ToDtoAsync(replacement.Id);
    }

    private async Task<WarrantyReplacementDto?> ToDtoAsync(Guid id)
    {
        var w = await _context.PT_WarrantyReplacements
            .Include(x => x.Vacancy)
            .Include(x => x.RequestedByUser)
            .Include(x => x.OriginalNegotiation).ThenInclude(n => n!.WinningApplication).ThenInclude(a => a!.Candidate)
            .Include(x => x.OriginalDelivery).ThenInclude(d => d!.Candidate)
            .Include(x => x.ReplacementNegotiation).ThenInclude(n => n!.WinningApplication).ThenInclude(a => a!.Candidate)
            .Include(x => x.ReplacementDelivery).ThenInclude(d => d!.Candidate)
            .Include(x => x.ChargeTranche)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (w == null) return null;

        static string? FormatCandidate(PTCandidate? c) => c != null ? $"{c.FirstName} {c.LastName}".Trim() : null;

        var originalCandidateName = w.OriginalNegotiation != null
            ? FormatCandidate(w.OriginalNegotiation.WinningApplication?.Candidate)
            : FormatCandidate(w.OriginalDelivery?.Candidate);

        var replacementCandidateName = w.ReplacementNegotiation != null
            ? FormatCandidate(w.ReplacementNegotiation.WinningApplication?.Candidate)
            : FormatCandidate(w.ReplacementDelivery?.Candidate);

        return new WarrantyReplacementDto
        {
            Id = w.Id,
            VacancyId = w.PT_VacancyId,
            VacancyTitle = w.Vacancy?.Title ?? "",
            ContractId = w.PT_VacancyContractId,
            OriginalNegotiationId = w.OriginalNegotiationId,
            OriginalDeliveryId = w.OriginalDeliveryId,
            OriginalCandidateName = originalCandidateName,
            Reason = w.Reason,
            Notes = w.Notes,
            IsExclusion = w.IsExclusion,
            ReplacementNumber = w.ReplacementNumber,
            Status = w.Status,
            RequestedByName = w.RequestedByUser?.Email ?? "",
            RequestedAt = w.RequestedAt,
            ReplacementNegotiationId = w.ReplacementNegotiationId,
            ReplacementDeliveryId = w.ReplacementDeliveryId,
            ReplacementCandidateName = replacementCandidateName,
            CompletedAt = w.CompletedAt,
            ChargeTrancheId = w.ChargeTrancheId,
            ChargeAmount = w.ChargeTranche?.Amount,
            ChargeStatus = w.ChargeTranche?.Status
        };
    }
}
