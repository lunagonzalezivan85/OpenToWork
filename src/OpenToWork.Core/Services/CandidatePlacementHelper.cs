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
/// una Cancelada no cuenta) ni una liberacion manual (PlacementEndedAt, "Liberar candidato"). Si deja
/// el puesto vuelve a estar disponible para otra entrega.
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
                d.PlacementEndedAt,
                CompanyName = d.Company.Name,
                VacancyTitle = d.Vacancy.Title
            })
            .ToListAsync();

        var hiredNegotiations = await context.PT_Negotiations
            .Where(n => !n.IsDeleted && n.Status == (int)NegotiationStatus.Cerrada
                && n.WinningApplication != null && n.PlacementEndedAt == null && candidateIds.Contains(n.WinningApplication.PT_CandidateId))
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
                .Where(d => d.Status == (int)DeliveryStatus.Hired && d.PlacementEndedAt == null && !replacedSet.Contains(d.Id))
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
                LastDeliveredAt = last?.DeliveredAt,
                LastDeliveryLeft = last != null && last.Status == (int)DeliveryStatus.Hired
                    && (last.PlacementEndedAt != null || replacedSet.Contains(last.Id))
            };
        }

        return result;
    }

    /// <summary>Misma regla que GetSummariesAsync pero como IQueryable de PTCandidate.Id, para
    /// filtrar en SQL (ranking, busqueda, postulaciones). Con exceptVacancyId no cuenta como
    /// "colocado en otra plaza" al que fue contratado justamente en esa vacante.</summary>
    public static IQueryable<Guid> PlacedCandidateIds(AppDbContext context, Guid? exceptVacancyId = null)
    {
        var activeReplacements = context.PT_WarrantyReplacements
            .Where(w => !w.IsDeleted && w.Status != (int)WarrantyReplacementStatus.Cancelada);

        var fromDeliveries = context.PT_CandidateDeliveries
            .Where(d => !d.IsDeleted && d.Status == (int)DeliveryStatus.Hired && d.PlacementEndedAt == null
                && (exceptVacancyId == null || d.PT_VacancyId != exceptVacancyId)
                && !activeReplacements.Any(w => w.OriginalDeliveryId == d.Id))
            .Select(d => d.PT_CandidateId);

        var fromNegotiations = context.PT_Negotiations
            .Where(n => !n.IsDeleted && n.Status == (int)NegotiationStatus.Cerrada && n.WinningApplicationId != null && n.PlacementEndedAt == null
                && (exceptVacancyId == null || n.PT_VacancyId != exceptVacancyId)
                && !activeReplacements.Any(w => w.OriginalNegotiationId == n.Id))
            .Select(n => n.WinningApplication!.PT_CandidateId);

        return fromDeliveries.Concat(fromNegotiations);
    }

    /// <summary>Por PTCandidate.Id. Para validar una accion puntual (postularse, negociar).</summary>
    public static Task<bool> IsCandidatePlacedAsync(AppDbContext context, Guid ptCandidateId, Guid? exceptVacancyId = null) =>
        PlacedCandidateIds(context, exceptVacancyId).AnyAsync(id => id == ptCandidateId);

    public const string PlacedErrorMessage =
        "El candidato ya esta Colocado (contratado en otra empresa). Vuelve a estar disponible si deja ese puesto (reposicion de garantia o \"Liberar candidato\" en el admin).";
}
