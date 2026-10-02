using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IAuthService
{
    /// <summary>Valida los datos (ArgumentException con el motivo si no son validos) y crea la cuenta.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? createdBy = null, string? consentIp = null);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<bool> RegisterDeviceAsync(Guid userId, string deviceHash, string? deviceName);
    Task<bool> IsDeviceKnownAsync(Guid userId, string deviceHash);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<AuthResponseDto?> GoogleLoginAsync(string googleToken);
    Task<bool> VerifyRecaptchaAsync(string recaptchaResponse);

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
    UserNotFound
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
