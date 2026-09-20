using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IVerificationRequestService
{
    /// <summary>
    /// Crea una solicitud de verificacion para el candidato del usuario autenticado.
    /// Guarda los documentos adjuntos en uploadsRoot y registra la traza de consentimiento.
    /// Devuelve null si el usuario no tiene perfil de candidato.
    /// </summary>
    Task<VerificationRequestResultDto?> SubmitAsync(Guid userId, SubmitVerificationRequestDto dto, string uploadsRoot, string? consentIp);

    /// <summary>Ultima solicitud del candidato (para mostrar estado en el dashboard).</summary>
    Task<VerificationRequestResultDto?> GetLatestAsync(Guid userId);
}
