using Microsoft.EntityFrameworkCore;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Regla unica de "Colocado" + resumen de entregas para las vistas del admin. Se calcula en vivo
/// (sin columna ni etapa nueva en la base): un candidato esta Colocado si tiene una entrega en
/// Contratado, o gano una negociacion Cerrada, sin una reposicion de garantia activa sobre ella
/// (una reposicion EnCurso/Completada/Excluida significa que el candidato ya no esta en ese puesto;
/// una Cancelada no cuenta). Si deja el puesto vuelve a estar disponible para otra entrega.
/// </summary>
internal static class CandidatePlacementHelper
{
    /// <summary>Dado un set de SCUser.Id de candidatos, devuelve su resumen keyed por SCUser.Id.
    /// Los que no tienen perfil de candidato ni entregas no aparecen en el diccionario.</summary>
    public static async Task<Dictionary<Guid, CandidatePlacementSummaryDto>> GetSummariesAsync(AppDbContext context, IEnumerable<Guid> userIds)
    {
        var result = new Dictionary<Guid, CandidatePlacementSummaryDto>();
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return result;

        var candidateUserMap = await context.PT_Candidates
            .Where(c => ids.Contains(c.SCUserId) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id, c => c.SCUserId);
        if (candidateUserMap.Count == 0) return result;
        var candidateIds = candidateUserMap.Keys.ToList();

        var deliveries = await context.PT_CandidateDeliveries
            .Where(d => candidateIds.Contains(d.PT_CandidateId) && !d.IsDeleted)
            .Select(d => new
            {
                d.Id,
                d.PT_CandidateId,
                d.Status,
                d.DeliveredAt,
                CompanyName = d.Company.Name,
                VacancyTitle = d.Vacancy.Title
            })
            .ToListAsync();

        var hiredNegotiations = await context.PT_Negotiations
            .Where(n => !n.IsDeleted && n.Status == (int)NegotiationStatus.Cerrada
                && n.WinningApplication != null && candidateIds.Contains(n.WinningApplication.PT_CandidateId))
            .Select(n => new
            {
                n.Id,
                CandidateId = n.WinningApplication!.PT_CandidateId,
                ClosedAt = n.ClosedAt ?? n.CreatedAt,
                CompanyName = n.Vacancy.Company.Name,
                VacancyTitle = n.Vacancy.Title
            })
            .ToListAsync();

        var deliveryIds = deliveries.Select(d => d.Id).ToList();
        var negotiationIds = hiredNegotiations.Select(n => n.Id).ToList();
        var replaced = await context.PT_WarrantyReplacements
            .Where(w => !w.IsDeleted && w.Status != (int)WarrantyReplacementStatus.Cancelada
                && ((w.OriginalDeliveryId != null && deliveryIds.Contains(w.OriginalDeliveryId.Value))
                    || (w.OriginalNegotiationId != null && negotiationIds.Contains(w.OriginalNegotiationId.Value))))
            .Select(w => w.OriginalDeliveryId ?? w.OriginalNegotiationId!.Value)
            .ToListAsync();
        var replacedSet = new HashSet<Guid>(replaced);

        foreach (var (candidateId, userId) in candidateUserMap)
        {
            var own = deliveries.Where(d => d.PT_CandidateId == candidateId).OrderByDescending(d => d.DeliveredAt).ToList();
            var placements = own
                .Where(d => d.Status == (int)DeliveryStatus.Hired && !replacedSet.Contains(d.Id))
                .Select(d => (d.DeliveredAt, d.CompanyName, d.VacancyTitle))
                .Concat(hiredNegotiations
                    .Where(n => n.CandidateId == candidateId && !replacedSet.Contains(n.Id))
                    .Select(n => (DeliveredAt: n.ClosedAt, n.CompanyName, n.VacancyTitle)))
                .OrderByDescending(p => p.DeliveredAt)
                .ToList();

            if (own.Count == 0 && placements.Count == 0) continue;

            var last = own.FirstOrDefault();
            var placed = placements.FirstOrDefault();
            result[userId] = new CandidatePlacementSummaryDto
            {
                IsPlaced = placements.Count > 0,
                PlacedCompanyName = placements.Count > 0 ? placed.CompanyName : null,
                PlacedVacancyTitle = placements.Count > 0 ? placed.VacancyTitle : null,
                DeliveriesCount = own.Count,
                RejectedCount = own.Count(d => d.Status == (int)DeliveryStatus.RejectedByCompany),
                LastDeliveryStatus = last?.Status,
                LastDeliveryCompanyName = last?.CompanyName,
                LastDeliveredAt = last?.DeliveredAt
            };
        }

        return result;
    }

    public static async Task<bool> IsPlacedAsync(AppDbContext context, Guid userId)
    {
        var summaries = await GetSummariesAsync(context, new[] { userId });
        return summaries.TryGetValue(userId, out var s) && s.IsPlaced;
    }
}
