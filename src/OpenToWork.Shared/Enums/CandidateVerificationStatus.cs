namespace OpenToWork.Shared.Enums;

/// <summary>
/// Estado progresivo de verificacion del candidato. Ver plan obligatorio de Fase 3 en README,
/// sub-fase 3.7, y las decisiones documentadas en docs/dsiezar/fase-3-sub7.md. Se calcula
/// siempre en vivo (nunca se persiste) - no hay una tabla/columna para este enum.
/// </summary>
public enum CandidateVerificationStatus
{
    ProfileRegistered = 0,
    ProfileComplete = 1,
    Evaluated = 2,
    InProgress = 3,
    VerifiedTD = 4
}
