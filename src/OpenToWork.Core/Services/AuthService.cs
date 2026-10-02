using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;
using OpenToWork.Shared.Validation;

namespace OpenToWork.Core.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ITokenCryptoService _tokenCrypto;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IEmailService _email;
    private readonly ILogger<AuthService> _logger;

    private const int MinPasswordLength = 6;
    private const int EmailCodeValidityMinutes = 15;
    private const int EmailCodeMaxAttempts = 5;
    private static readonly TimeSpan EmailCodeResendCooldown = TimeSpan.FromMinutes(1);

    public AuthService(AppDbContext context, IConfiguration config, ITokenCryptoService tokenCrypto, IGoogleTokenValidator googleTokenValidator,
        IEmailService email, ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _tokenCrypto = tokenCrypto;
        _googleTokenValidator = googleTokenValidator;
        _email = email;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? createdBy = null, string? consentIp = null)
    {
        var isCandidate = dto.PrimaryRole == (int)UserRole.Candidate;
        var firstName = dto.FirstName?.Trim() ?? string.Empty;
        var lastName = dto.LastName?.Trim() ?? string.Empty;
        var identification = string.IsNullOrWhiteSpace(dto.Identification) ? null : dto.Identification.Trim();

        // El formulario ya avisa de todo esto; aqui se repite porque la API es la que manda.
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            throw new ArgumentException("El correo electrónico no es válido.");
        if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < MinPasswordLength)
            throw new ArgumentException($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");
        if (!dto.AcceptPrivacy)
            throw new ArgumentException("Debes aceptar la política de privacidad.");
        if (isCandidate)
        {
            if (firstName.Length == 0 || lastName.Length == 0)
                throw new ArgumentException("El nombre y los apellidos son obligatorios.");
            if (firstName.Length > 100 || lastName.Length > 100)
                throw new ArgumentException("El nombre o los apellidos son demasiado largos.");
            if (dto.DocumentType is not int docType || !Enum.IsDefined(typeof(IdentityDocumentType), docType))
                throw new ArgumentException("Selecciona el tipo de documento.");
            if (!IdentityDocumentValidator.IsValid((IdentityDocumentType)docType, identification))
                throw new ArgumentException("El número de documento no es válido para el tipo seleccionado.");
            identification = IdentityDocumentValidator.Normalize(identification);
        }

        var companyName = dto.CompanyName?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        if (dto.PrimaryRole == (int)UserRole.Company)
        {
            if (companyName.Length == 0)
                throw new ArgumentException("El nombre de la empresa es obligatorio.");
            if (companyName.Length > 200)
                throw new ArgumentException("El nombre de la empresa es demasiado largo.");
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("El NIF de la empresa es obligatorio.");
            if (!IdentityDocumentValidator.IsValidCompanyNif(identification))
                throw new ArgumentException("El NIF de la empresa no es válido.");
            identification = IdentityDocumentValidator.Normalize(identification);
            if (!PhoneValidator.IsValid(phone))
                throw new ArgumentException("El teléfono de la empresa no es válido (entre 9 y 15 dígitos).");
            phone = PhoneValidator.Normalize(phone);
        }

        var existing = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (existing != null)
            throw new InvalidOperationException("Email already registered");

        // Candidato: la cuenta solo se crea con el codigo que se envio al correo (decision de Darwin 1-Oct).
        SCEmailVerificationCode? pendingCode = null;
        if (isCandidate)
            pendingCode = await ConsumeRegistrationCodeAsync(dto.Email, dto.EmailCode);

        var now = DateTime.UtcNow;
        var user = new SCUser
        {
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PrimaryRole = dto.PrimaryRole,
            FullName = isCandidate ? $"{firstName} {lastName}" : null,
            Identification = identification,
            Phone = phone,
            EmailVerified = isCandidate,
            IsActive = true,
            CreatedBy = createdBy != null ? Guid.Parse(createdBy) : null,
            PrivacyAcceptedAt = now,
            PrivacyPolicyVersion = LegalVersions.Privacy,
            ConsentIp = consentIp,
            MarketingConsent = dto.AcceptMarketing,
            MarketingConsentAt = now
        };

        user.UserRoles.Add(new SCUserRole { Role = dto.PrimaryRole, SCUserId = user.Id });
        user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };

        if (isCandidate)
        {
            user.Candidate = new PTCandidate
            {
                SCUserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Identification = identification,
                DocumentType = dto.DocumentType,
                Phone = phone,
                WizardStep = 0,
                WizardCompleted = false
            };
        }
        else if (dto.PrimaryRole == 1)
        {
            // El NIF va a la ficha de la empresa: es el que se usa en contratos y en su perfil.
            user.Company = new PTCompany
            {
                SCUserId = user.Id,
                Name = companyName,
                TaxId = identification,
                ContactEmail = user.Email,
                ContactPhone = phone
            };
        }

        _context.SC_Users.Add(user);
        if (pendingCode != null)
            _context.SC_EmailVerificationCodes.Remove(pendingCode);
        await _context.SaveChangesAsync();

        // Empresa: se verifica despues (no bloquea). Si el correo no sale, puede pedir otro codigo desde el portal.
        if (!isCandidate)
            await SendEmailVerificationCodeAsync(user.Id);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<SendVerificationCodeResult> SendRegistrationCodeAsync(string email, string? firstName = null)
    {
        var normalized = NormalizeEmail(email);
        if (normalized.Length == 0 || normalized.Length > 256 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(normalized))
            return SendVerificationCodeResult.InvalidEmail;

        if (await _context.SC_Users.AnyAsync(u => u.Email == normalized && !u.IsDeleted))
            return SendVerificationCodeResult.EmailAlreadyRegistered;

        var pending = await _context.SC_EmailVerificationCodes.FirstOrDefaultAsync(c => c.Email == normalized);
        if (pending != null && DateTime.UtcNow - pending.LastSentAt < EmailCodeResendCooldown)
            return SendVerificationCodeResult.TooSoon;

        if (pending == null)
        {
            pending = new SCEmailVerificationCode { Email = normalized };
            _context.SC_EmailVerificationCodes.Add(pending);
        }

        var code = NewEmailCode();
        pending.CodeHash = HashRegistrationCode(normalized, code);
        pending.ExpiresAt = DateTime.UtcNow.AddMinutes(EmailCodeValidityMinutes);
        pending.LastSentAt = DateTime.UtcNow;
        pending.Attempts = 0;
        pending.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Solo para el saludo del correo; el nombre se valida y guarda despues, en RegisterAsync.
        var name = firstName?.Trim();
        if (name?.Length > 100) name = null;

        return await SendCodeEmailAsync(normalized, name, code)
            ? SendVerificationCodeResult.Sent
            : SendVerificationCodeResult.SendFailed;
    }

    /// <summary>
    /// Comprueba el codigo del registro. Si no vale, guarda el intento fallido y lanza ArgumentException
    /// con el motivo (el controller lo devuelve como 400). Si vale, devuelve la fila para borrarla al
    /// crear la cuenta (en el mismo SaveChanges, asi no queda cuenta sin borrar el codigo ni al reves).
    /// </summary>
    private async Task<SCEmailVerificationCode> ConsumeRegistrationCodeAsync(string email, string? code)
    {
        var normalized = NormalizeEmail(email);
        var pending = await _context.SC_EmailVerificationCodes.FirstOrDefaultAsync(c => c.Email == normalized);

        if (pending == null || pending.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("El código venció o no se pidió. Pide uno nuevo.");
        if (pending.Attempts >= EmailCodeMaxAttempts)
            throw new ArgumentException("Demasiados intentos. Pide un código nuevo.");

        var expected = Encoding.UTF8.GetBytes(pending.CodeHash);
        var actual = Encoding.UTF8.GetBytes(HashRegistrationCode(normalized, (code ?? string.Empty).Trim()));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            pending.Attempts++;
            await _context.SaveChangesAsync();
            throw new ArgumentException(pending.Attempts >= EmailCodeMaxAttempts
                ? "Demasiados intentos. Pide un código nuevo."
                : "El código no es correcto.");
        }

        return pending;
    }

    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static string NewEmailCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    // Con el correo como sal (aun no hay id de usuario).
    private string HashRegistrationCode(string normalizedEmail, string code) => _tokenCrypto.HashToken($"register:{normalizedEmail}:{code}");

    /// <summary>Envia el correo con el codigo. En desarrollo, si el SMTP esta apagado, escribe el codigo en el log.</summary>
    private async Task<bool> SendCodeEmailAsync(string toEmail, string? name, string code)
    {
        var html = EmailTemplates.VerificationCode(name, code, EmailCodeValidityMinutes);
        var (sent, error) = await _email.SendAsync(toEmail, name, $"{code} es tu código de verificación de Trato Directo", html);
        if (sent) return true;

        // Solo con Auth:LogVerificationCodes (lo activa Program.cs en Development): permite probar el registro sin SMTP.
        if (_config.GetValue<bool>("Auth:LogVerificationCodes"))
        {
            _logger.LogWarning("[DEV] SMTP no disponible ({Error}). Codigo de verificacion para {Email}: {Code}", error, toEmail, code);
            return true;
        }

        _logger.LogWarning("No se pudo enviar el codigo de verificacion: {Error}", error);
        return false;
    }

    public async Task<EmailVerificationStatusDto?> GetEmailVerificationStatusAsync(Guid userId)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        return user == null ? null : new EmailVerificationStatusDto { Email = user.Email, EmailVerified = user.EmailVerified };
    }

    public async Task<SendVerificationCodeResult> SendEmailVerificationCodeAsync(Guid userId)
    {
        var user = await _context.SC_Users
            .Include(u => u.Candidate)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
        if (user == null) return SendVerificationCodeResult.UserNotFound;
        if (user.EmailVerified) return SendVerificationCodeResult.AlreadyVerified;

        var issuedAt = user.EmailVerificationExpiresAt?.AddMinutes(-EmailCodeValidityMinutes);
        if (issuedAt != null && DateTime.UtcNow - issuedAt < EmailCodeResendCooldown)
            return SendVerificationCodeResult.TooSoon;

        var code = NewEmailCode();
        user.EmailVerificationCodeHash = HashEmailCode(user.Id, code);
        user.EmailVerificationExpiresAt = DateTime.UtcNow.AddMinutes(EmailCodeValidityMinutes);
        user.EmailVerificationAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await SendCodeEmailAsync(user.Email, user.Candidate?.FirstName, code)
            ? SendVerificationCodeResult.Sent
            : SendVerificationCodeResult.SendFailed;
    }

    public async Task<EmailVerificationResult> VerifyEmailAsync(Guid userId, string code)
    {
        var user = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive);
        if (user == null) return EmailVerificationResult.UserNotFound;
        if (user.EmailVerified) return EmailVerificationResult.AlreadyVerified;
        if (user.EmailVerificationCodeHash == null || user.EmailVerificationExpiresAt == null || user.EmailVerificationExpiresAt < DateTime.UtcNow)
            return EmailVerificationResult.Expired;
        if (user.EmailVerificationAttempts >= EmailCodeMaxAttempts)
            return EmailVerificationResult.TooManyAttempts;

        var expected = Encoding.UTF8.GetBytes(user.EmailVerificationCodeHash);
        var actual = Encoding.UTF8.GetBytes(HashEmailCode(user.Id, (code ?? string.Empty).Trim()));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            user.EmailVerificationAttempts++;
            await _context.SaveChangesAsync();
            return user.EmailVerificationAttempts >= EmailCodeMaxAttempts
                ? EmailVerificationResult.TooManyAttempts
                : EmailVerificationResult.Invalid;
        }

        user.EmailVerified = true;
        user.EmailVerificationCodeHash = null;
        user.EmailVerificationExpiresAt = null;
        user.EmailVerificationAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return EmailVerificationResult.Verified;
    }

    // Con el id del usuario como sal: el mismo codigo de 6 digitos da hashes distintos en cada cuenta.
    private string HashEmailCode(Guid userId, string code) => _tokenCrypto.HashToken($"{userId:N}:{code}");

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.SC_Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserPreference)
            .Include(u => u.Candidate)
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!string.IsNullOrEmpty(dto.DeviceHash))
        {
            await RegisterDeviceAsync(user.Id, dto.DeviceHash, dto.DeviceName);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user, dto.RememberMe);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var tokenHash = _tokenCrypto.HashToken(dto.RefreshToken);
        var token = await _context.SC_RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles)
            .Include(t => t.User).ThenInclude(u => u.UserPreference)
            .Include(t => t.User).ThenInclude(u => u.Candidate)
            .Include(t => t.User).ThenInclude(u => u.Company)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsRevoked && !t.IsDeleted);

        if (token == null || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        token.IsRevoked = true;

        return await GenerateAuthResponseAsync(token.User);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var tokenHash = _tokenCrypto.HashToken(refreshToken);
        var token = await _context.SC_RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsRevoked && !t.IsDeleted);

        if (token == null) return false;

        token.IsRevoked = true;
        token.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegisterDeviceAsync(Guid userId, string deviceHash, string? deviceName)
    {
        var device = await _context.SC_UserDevices
            .FirstOrDefaultAsync(d => d.SCUserId == userId && d.DeviceHash == deviceHash && !d.IsDeleted);

        if (device != null)
        {
            device.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return false;
        }

        _context.SC_UserDevices.Add(new SCUserDevice
        {
            SCUserId = userId,
            DeviceHash = deviceHash,
            DeviceName = deviceName,
            FirstSeenAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsTrusted = false
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsDeviceKnownAsync(Guid userId, string deviceHash)
    {
        return await _context.SC_UserDevices
            .AnyAsync(d => d.SCUserId == userId && d.DeviceHash == deviceHash && !d.IsDeleted);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(SCUser user, bool rememberMe = false)
    {
        var token = GenerateJwtToken(user, rememberMe);
        var refreshToken = _tokenCrypto.GenerateRefreshToken();
        var refreshTokenHash = _tokenCrypto.HashToken(refreshToken);

        var expireDays = _config.GetValue<int>("Jwt:RefreshTokenExpireDays", 7);

        _context.SC_RefreshTokens.Add(new SCRefreshToken
        {
            SCUserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
            IsRevoked = false
        });
        await _context.SaveChangesAsync();

        var expireMinutes = rememberMe
            ? _config.GetValue<int>("Jwt:ExpireMinutesRememberMe", 43200)
            : _config.GetValue<int>("Jwt:ExpireMinutes", 60);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                PrimaryRole = user.PrimaryRole,
                Identification = user.Identification,
                Phone = user.Phone,
                EmailVerified = user.EmailVerified,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(r => r.Role).ToList(),
                Theme = user.UserPreference?.Theme,
                Language = user.UserPreference?.Language,
                PreferredRole = user.UserPreference?.PreferredRole,
                WizardCompleted = user.Candidate?.WizardCompleted ?? false,
                WizardStep = user.Candidate?.WizardStep ?? 0
            }
        };
    }

    private string GenerateJwtToken(SCUser user, bool rememberMe)
    {
        var expireMinutes = rememberMe
            ? _config.GetValue<int>("Jwt:ExpireMinutesRememberMe", 43200)
            : _config.GetValue<int>("Jwt:ExpireMinutes", 60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("primaryRole", user.PrimaryRole.ToString()),
            new(JwtRegisteredClaimNames.GivenName, user.Company?.Name ?? user.Candidate?.FirstName ?? user.Email.Split('@')[0]),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Apellido del candidato: el avatar del menu muestra primer nombre + primer apellido.
        if (!string.IsNullOrWhiteSpace(user.Candidate?.LastName))
            claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.Candidate.LastName));

        foreach (var role in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, ((OpenToWork.Shared.Enums.UserRole)role.Role).ToString()));
        }

        return _tokenCrypto.CreateJwtToken(claims, _config["Jwt:Key"]!, _config["Jwt:Issuer"]!, _config["Jwt:Audience"]!, expireMinutes);
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive);

        if (user == null) return false;

        var resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = _tokenCrypto.HashToken(resetToken);
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send email with reset link containing the token
        // For now, the token is returned via the API response in production this would be emailed
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var tokenHash = _tokenCrypto.HashToken(token);
        var user = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == tokenHash && !u.IsDeleted && u.IsActive);

        if (user == null || user.PasswordResetExpiresAt == null || user.PasswordResetExpiresAt < DateTime.UtcNow)
            return false;
        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6) // mismo minimo que RegisterDto
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;
        // Usar el enlace recibido por correo (recuperacion o invitacion de empresa) prueba el email.
        user.EmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(string googleToken)
    {
        // Revision de seguridad 25-Sep: antes se decodificaba el payload SIN verificar la firma y
        // cualquiera entraba a una cuenta ajena fabricando un token con su correo. Ahora el token se
        // valida contra Google (firma, emisor, audiencia, vigencia) en GoogleTokenValidator.
        var identity = await _googleTokenValidator.ValidateAsync(googleToken);
        if (identity == null) return null;

        var email = identity.Email;
        var googleId = identity.Subject;

        try
        {
            var user = await _context.SC_Users
                .Include(u => u.UserRoles)
                .Include(u => u.UserPreference)
                .Include(u => u.Candidate)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted);

            if (user == null)
            {
                // Check if email exists - link GoogleId to existing account
                user = await _context.SC_Users
                    .Include(u => u.UserRoles)
                    .Include(u => u.UserPreference)
                    .Include(u => u.Candidate)
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

                if (user == null)
                {
                    // Create new user from Google info
                    user = new SCUser
                    {
                        Email = email,
                        GoogleId = googleId,
                        PrimaryRole = 0,
                        EmailVerified = true,
                        IsActive = true
                    };
                    user.UserRoles.Add(new SCUserRole { Role = 0, SCUserId = user.Id });
                    user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };
                    user.Candidate = new PTCandidate { SCUserId = user.Id, WizardStep = 0, WizardCompleted = false };

                    _context.SC_Users.Add(user);
                }
                else
                {
                    // Solo se vincula a una cuenta existente si Google garantiza que el correo es de
                    // quien inicia sesion; si no, cualquiera con una cuenta de Google con ese correo
                    // sin verificar podria apropiarse de la cuenta.
                    if (!identity.EmailVerified) return null;
                    user.GoogleId = googleId;
                    user.EmailVerified = true;
                }
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GenerateAuthResponseAsync(user);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
    {
        var secretKey = _config["Recaptcha:SecretKey"];
        if (string.IsNullOrEmpty(secretKey)) return true; // Skip if not configured

        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", secretKey },
            { "response", recaptchaResponse }
        });

        var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
        return result?.Success ?? false;
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public string[]? ErrorCodes { get; set; }
    }
}
