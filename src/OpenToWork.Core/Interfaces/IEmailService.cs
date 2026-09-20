namespace OpenToWork.Core.Interfaces;

public interface IEmailService
{
    /// <summary>Envia un correo via SMTP usando la configuracion guardada en SY_SystemConfig.
    /// Si SMTP no esta habilitado o no esta configurado, no intenta conectar y devuelve
    /// Success=false con el motivo en Error (nunca lanza excepcion por eso).</summary>
    Task<(bool Success, string? Error)> SendAsync(string toEmail, string? toName, string subject, string htmlBody);
}
