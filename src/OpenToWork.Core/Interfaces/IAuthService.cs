using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAuthService
{
    /// <summary>Valida los datos (ArgumentException con el motivo si no son validos) y crea la cuenta.</summary>
    /// <param name="google">Registro con Google: solo candidato; el correo sale de Google y no hace falta contrasena ni codigo.</param>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? createdBy = null, string? consentIp = null, GoogleIdentity? google = null);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<int> RevokeAllTokensAsync(Guid userId);
    Task<bool> RegisterDeviceAsync(Guid userId, string deviceHash, string? deviceName);
    Task<bool> IsDeviceKnownAsync(Guid userId, string deviceHash);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    /// <summary>Inicio de sesion con Google, solo candidatos. NeedsSignup: no hay cuenta, falta completar registro.</summary>
    Task<(GoogleSignInStatus Status, AuthResponseDto? Auth)> GoogleSignInAsync(GoogleIdentity identity);
    Task<bool> VerifyRecaptchaAsync(string recaptchaResponse);

    /// <summary>Registro de candidato, paso 1: envia el codigo al correo sin crear la cuenta.</summary>
    Task<SendVerificationCodeResult> SendRegistrationCodeAsync(string email, string? firstName = null, string? recaptchaToken = null);

    Task<EmailVerificationStatusDto?> GetEmailVerificationStatusAsync(Guid userId);
    /// <summary>Genera un codigo nuevo y lo envia por correo.</summary>
    Task<SendVerificationCodeResult> SendEmailVerificationCodeAsync(Guid userId);
    Task<EmailVerificationResult> VerifyEmailAsync(Guid userId, string code);
}

public enum SendVerificationCodeResult
{
    Sent,
    AlreadyVerified,
    /// <summary>Se pidio otro codigo hace menos de un minuto.</summary>
    TooSoon,
    /// <summary>El SMTP fallo o esta deshabilitado.</summary>
    SendFailed,
    UserNotFound,
    /// <summary>Registro: ya hay una cuenta con ese correo.</summary>
    EmailAlreadyRegistered,
    InvalidEmail,
    /// <summary>Con Recaptcha:Enforced activo, el token falta o no es valido.</summary>
    CaptchaFailed
}

public enum GoogleSignInStatus
{
    SignedIn,
    NeedsSignup,
    /// <summary>El correo es de una empresa o del staff: Google solo vale para candidatos.</summary>
    NotCandidate,
    Inactive,
    EmailNotVerified
}

public enum EmailVerificationResult
{
    Verified,
    AlreadyVerified,
    Invalid,
    Expired,
    TooManyAttempts,
    UserNotFound
}
