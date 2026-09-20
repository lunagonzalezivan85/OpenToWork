using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

public class EmailService : IEmailService
{
    private readonly ISystemConfigService _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ISystemConfigService config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> SendAsync(string toEmail, string? toName, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return (false, "Falta el correo del destinatario.");

        var settings = await _config.GetSmtpCredentialsAsync();

        if (!settings.Enabled)
            return (false, "El envio de correos esta deshabilitado en Configuracion > Notificaciones por Email.");

        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress))
            return (false, "El SMTP no esta configurado (falta servidor o correo remitente).");

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
            message.To.Add(new MailboxAddress(toName ?? toEmail, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            var socketOptions = settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(settings.Host, settings.Port, socketOptions);

            if (!string.IsNullOrWhiteSpace(settings.Username))
                await client.AuthenticateAsync(settings.Username, settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al enviar correo SMTP a {ToEmail}", toEmail);
            return (false, ex.Message);
        }
    }
}
