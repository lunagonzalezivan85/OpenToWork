namespace OpenToWork.Shared.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>Codigo de un solo uso con el que el portal recoge la sesion tras volver de Google.</summary>
public class GoogleExchangeDto
{
    public string Code { get; set; } = string.Empty;
}

/// <summary>Completar registro tras Google: mismos datos de candidato que el registro normal, sin correo,
/// contrasena ni codigo (el correo lo pone Google). Ticket = el que trae el portal en la URL.</summary>
public class GoogleSignupDto : RegisterDto
{
    public string Ticket { get; set; } = string.Empty;
}

/// <summary>Lo que Google ya nos dio, para rellenar el formulario de completar registro.</summary>
public class GoogleSignupInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class VerifyRecaptchaDto
{
    public string Response { get; set; } = string.Empty;
}
