using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface ISystemConfigService
{
    Task<List<SystemConfigDto>> GetAllAsync();
    Task UpdateBulkAsync(UpdateSystemConfigDto dto, Guid staffId);
    Task<CompanyIdentityDto> GetCompanyIdentityAsync();
    Task<SmtpSettingsDto> GetSmtpSettingsAsync();
    Task UpdateSmtpSettingsAsync(SmtpSettingsDto dto, Guid staffId);

    /// <summary>Igual que GetSmtpSettingsAsync pero incluye la contrasena real. Solo para uso
    /// interno de IEmailService al enviar - nunca exponer via controller/API.</summary>
    Task<SmtpSettingsDto> GetSmtpCredentialsAsync();

    /// <summary>Configuracion del proveedor de IA (sin la API key - HasApiKey indica si existe una).</summary>
    Task<AiSettingsDto> GetAiSettingsAsync();
    Task UpdateAiSettingsAsync(AiSettingsDto dto, Guid staffId);

    /// <summary>Igual que GetAiSettingsAsync pero incluye la API key real. Solo para uso interno de los
    /// servicios que consumen IA (analisis de CV, command bar, etc.) - nunca exponer via controller/API.</summary>
    Task<AiSettingsDto> GetAiCredentialsAsync();

    /// <summary>Flag apagado por defecto: mientras este en false, los planes de candidato
    /// (audience=Candidate) no se muestran en el portal publico aunque existan filas activas.</summary>
    Task<bool> GetCandidatePriorityPlanEnabledAsync();
    Task SetCandidatePriorityPlanEnabledAsync(bool enabled, Guid staffId);

    /// <summary>Independiente del flag de candidatos. Encendido por defecto (los planes de empresa
    /// ya estaban visibles antes de que este flag existiera).</summary>
    Task<bool> GetCompanyPlansEnabledAsync();
    Task SetCompanyPlansEnabledAsync(bool enabled, Guid staffId);
}
