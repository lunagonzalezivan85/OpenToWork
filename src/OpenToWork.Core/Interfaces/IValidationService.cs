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

    /// <summary>Lectura pura de lo ya persistido en PT_Verifications - no dispara HTTP (agregado en sub-fase 3.8 para listar sin recalcular en cada carga de pantalla).</summary>
    Task<List<VerificationResultDto>> GetVerificationsAsync(Guid candidateId);

    /// <summary>
    /// Override manual de un admin (aprobar/rechazar) - "verificaciones manuales", el item de
    /// Fase 4 desbloqueado desde fase-3-sub1.md. status debe ser Verified o Failed
    /// (VerificationCheckStatus); Score se fija en 100/0 respectivamente.
    /// </summary>
    Task<VerificationResultDto> SetVerificationStatusAsync(Guid candidateId, int type, int status, Guid adminId);
}
