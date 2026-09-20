using Microsoft.AspNetCore.Mvc;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminAPI.Controllers;

/// <summary>Configuracion SMTP (SY_SystemConfig, categoria "Smtp") y envio de correo de prueba.</summary>
[Route("api/admin/email")]
[RequireStaffRole]
public class EmailController : AdminControllerBase
{
    private readonly ISystemConfigService _config;
    private readonly IEmailService _email;

    public EmailController(ISystemConfigService config, IEmailService email)
    {
        _config = config;
        _email = email;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _config.GetSmtpSettingsAsync();
        return Ok(result);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] SmtpSettingsDto dto)
    {
        await _config.UpdateSmtpSettingsAsync(dto, AdminId);
        return Ok(await _config.GetSmtpSettingsAsync());
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestEmailDto dto)
    {
        var (success, error) = await _email.SendAsync(dto.ToEmail, null,
            "Correo de prueba - Trato Directo",
            "<p>Este es un correo de prueba enviado desde la configuracion de Notificaciones por Email del panel administrativo de Trato Directo.</p>");

        return success ? Ok(new { success = true }) : BadRequest(new { error });
    }
}
