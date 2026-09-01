using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Verificaciones automaticas del candidato, sin intervencion humana. Ver plan obligatorio
/// de Fase 3 en README, sub-fase 3.2, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub2.md.
/// </summary>
public interface IValidationService
{
    Task<VerificationResultDto> VerifyLinkedInAsync(Guid candidateId);
    Task<VerificationResultDto> VerifyPortfolioAsync(Guid candidateId);
    Task<VerificationResultDto> VerifyCvCoherenceAsync(Guid candidateId);
    Task<VerificationResultDto> VerifyIdentityAsync(Guid candidateId);
    Task<List<string>> DetectRedFlagsAsync(Guid candidateId);
    Task<List<VerificationResultDto>> RunAllVerificationsAsync(Guid candidateId);
}
