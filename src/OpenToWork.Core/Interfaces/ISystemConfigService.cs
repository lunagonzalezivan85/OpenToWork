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
}
