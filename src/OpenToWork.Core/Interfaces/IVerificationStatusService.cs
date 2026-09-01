using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Estado progresivo de verificacion (Perfil registrado -> ... -> Verificado TD). Ver plan
/// obligatorio de Fase 3 en README, sub-fase 3.7, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub7.md. Ambos metodos hacen lo mismo (calculo en vivo, nunca
/// persistido) - se mantienen separados porque el plan los lista como dos metodos distintos.
/// </summary>
public interface IVerificationStatusService
{
    Task<VerificationStatusDto> GetVerificationStatusAsync(Guid candidateId);
    Task<VerificationStatusDto> EvaluateVerificationStatusAsync(Guid candidateId);
}
