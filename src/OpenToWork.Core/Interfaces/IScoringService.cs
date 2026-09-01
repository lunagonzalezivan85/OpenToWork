using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Calcula los 4 indices del Candidate Score (intrinseco, no depende de ninguna vacante
/// puntual - ver PTJobMatchScore para el score por par candidato-vacante, sub-fase 3.4).
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.3, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub3.md.
///
/// Los 4 Calculate*Index son lecturas puras (sin HTTP) para que RecalculateAsync pueda
/// dispararse en cada edicion de perfil/experiencia/educacion sin bloquear el guardado.
/// EvidenceIndex lee el resultado ya persistido de PT_Verifications (IValidationService hace
/// las llamadas HTTP por separado, bajo demanda via POST verifications/run).
/// </summary>
public interface IScoringService
{
    Task<int> CalculateStabilityIndex(Guid candidateId);
    Task<int> CalculateReliabilityIndex(Guid candidateId);
    Task<int> CalculateEvidenceIndex(Guid candidateId);
    Task<int> CalculateCompatibilityIndex(Guid candidateId);
    int CalculateOverallScore(int stability, int reliability, int evidence, int compatibility);
    Task<CandidateScoreDto> RecalculateAsync(Guid candidateId);
    Task RecalculateAllAsync();
}
