using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Job Match Score: por par candidato-vacante (distinto del Candidate Score intrinseco de
/// IScoringService). Ver plan obligatorio de Fase 3 en README, sub-fase 3.4, y las decisiones
/// documentadas en docs/dsiezar/fase-3-sub4.md.
/// </summary>
public interface ICompatibilityService
{
    Task<JobMatchDto> CalculateJobMatch(Guid candidateId, Guid vacancyId);

    /// <summary>
    /// Calcula (y persiste) el match contra todos los candidatos elegibles (perfil publico +
    /// wizard completo). Devuelve la cantidad de candidatos evaluados.
    /// </summary>
    Task<int> CalculateMatchesForVacancyAsync(Guid vacancyId);

    Task<List<JobMatchDto>> GenerateShortlist(Guid vacancyId, int? limit = null);
}
