using Microsoft.EntityFrameworkCore;
using OpenToWork.Models.Context;

namespace OpenToWork.Core.Services;

/// <summary>
/// Regla unica de "Verificado" para listas del admin: reclutamiento en etapa 4
/// (Verificado manual) O checks automaticos completos (4 gating verificados +
/// score >= 70 + referencia verificada). Misma regla que AdminCandidateService
/// usa para IsVerifiedTD en la consola de candidatos.
/// </summary>
internal static class VerifiedCandidateHelper
{
    private static readonly List<int> GatingTypes = new() { 1, 2, 3, 5 }; // LinkedIn, Portfolio, CvCoherence, Reference

    /// <summary>Dado un set de PTCandidate.Id, devuelve los que estan verificados.</summary>
    public static async Task<HashSet<Guid>> GetVerifiedIdsAsync(AppDbContext context, List<Guid> ptCandidateIds)
    {
        var result = new HashSet<Guid>();
        if (ptCandidateIds.Count == 0) return result;

        // Map PTCandidate.Id -> SCUserId (el reclutamiento se keyed por SCUserId)
        var candidateUserMap = await context.PT_Candidates
            .Where(c => ptCandidateIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id, c => c.SCUserId);
        var userIds = candidateUserMap.Values.ToList();

        var verifiedUserIds = await context.PT_CandidateRecruitments
            .Where(r => userIds.Contains(r.SCUserId) && !r.IsDeleted && r.CurrentStage == 4)
            .Select(r => r.SCUserId)
            .ToListAsync();
        var verifiedUserSet = new HashSet<Guid>(verifiedUserIds);

        var verifications = await context.PT_Verifications
            .Where(v => ptCandidateIds.Contains(v.PT_CandidateId) && !v.IsDeleted && GatingTypes.Contains(v.Type))
            .ToListAsync();
        var scores = await context.PT_CandidateScores
            .Where(s => ptCandidateIds.Contains(s.PT_CandidateId) && !s.IsDeleted)
            .ToDictionaryAsync(s => s.PT_CandidateId, s => s.OverallScore);
        var verifiedRefs = await context.PT_CandidateReferences
            .Where(r => ptCandidateIds.Contains(r.PT_CandidateId) && !r.IsDeleted && r.Status == 3)
            .Select(r => r.PT_CandidateId)
            .Distinct()
            .ToListAsync();
        var verifiedRefSet = new HashSet<Guid>(verifiedRefs);

        foreach (var ptId in ptCandidateIds)
        {
            if (candidateUserMap.TryGetValue(ptId, out var userId) && verifiedUserSet.Contains(userId))
            {
                result.Add(ptId);
                continue;
            }

            var candidateVerifs = verifications.Where(v => v.PT_CandidateId == ptId).ToList();
            var allGatingVerified = GatingTypes.All(gt => candidateVerifs.Any(v => v.Type == gt && v.Status == 2));
            var scoreOk = scores.TryGetValue(ptId, out var sc) && sc >= 70;
            if (allGatingVerified && scoreOk && verifiedRefSet.Contains(ptId))
                result.Add(ptId);
        }

        return result;
    }
}
