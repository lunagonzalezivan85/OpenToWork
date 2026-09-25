namespace OpenToWork.Shared.DTOs;

/// <summary>Estado del acceso al portal (OpenToWork.WEB) de una empresa creada desde el admin.</summary>
public class CompanyPortalAccessDto
{
    /// <summary>La empresa tiene usuario vinculado (SCUserId).</summary>
    public bool HasAccount { get; set; }
    /// <summary>El usuario ya eligio su contraseña (acepto la invitacion).</summary>
    public bool IsActivated { get; set; }
    public string? Email { get; set; }
    public DateTime? InviteExpiresAt { get; set; }
}

/// <summary>Resultado de invitar (o reinvitar) a una empresa al portal.</summary>
public class CompanyPortalInviteResultDto
{
    public string Email { get; set; } = string.Empty;
    /// <summary>Enlace para que la empresa elija su contraseña. Se muestra una sola vez al admin
    /// (en la base solo queda el hash del token).</summary>
    public string InviteUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool EmailSent { get; set; }
    /// <summary>Por que no se envio el correo (SMTP apagado, sin configurar, error).</summary>
    public string? EmailError { get; set; }
}
