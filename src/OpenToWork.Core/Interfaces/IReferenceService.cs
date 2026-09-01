using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Referencias laborales que el candidato aporta desde su perfil (PTCandidateReference,
/// distinta de PTReferenceCheck de Iluna). Ver plan obligatorio de Fase 3 en README, sub-fase
/// 3.5, y las decisiones documentadas en docs/dsiezar/fase-3-sub5.md.
/// </summary>
public interface IReferenceService
{
    Task<CandidateReferenceDto> AddReferenceAsync(Guid candidateId, CreateReferenceDto dto);

    /// <summary>
    /// candidateId es el dueno esperado (dueno del perfil autenticado) - devuelve null si la
    /// referencia no existe o no le pertenece, mismo guard de ownership que /verifications/run
    /// y /score/recalculate, aplicado aca dentro del servicio porque la ruta HTTP
    /// (api/references/{id}/send) no trae un candidateId propio para comparar en el controller.
    /// </summary>
    Task<ReferenceRequestLinkDto?> SendReferenceRequestAsync(Guid candidateId, Guid referenceId);
    Task<bool> SubmitReferenceFeedbackAsync(string token, int rating, string? feedback);
    Task<CandidateReferenceDto?> VerifyReferenceAsync(Guid referenceId);
    Task<CandidateReferencesListDto> GetReferencesAsync(Guid candidateId);
}
