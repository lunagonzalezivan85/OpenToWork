using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ITokenCryptoService _tokenCrypto;

    public AuthService(AppDbContext context, IConfiguration config, ITokenCryptoService tokenCrypto)
    {
        _context = context;
        _config = config;
        _tokenCrypto = tokenCrypto;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? createdBy = null)
    {
        var existing = await _context.SC_Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (existing != null)
            throw new InvalidOperationException("Email already registered");

        var user = new SCUser
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PrimaryRole = dto.PrimaryRole,
            Identification = dto.Identification,
            Phone = dto.Phone,
            EmailVerified = false,
            IsActive = true,
            CreatedBy = createdBy != null ? Guid.Parse(createdBy) : null
        };

        user.UserRoles.Add(new SCUserRole { Role = dto.PrimaryRole, SCUserId = user.Id });
        user.UserPreference = new SYUserPreference { SCUserId = user.Id, Theme = "navy", Language = "es" };

        if (dto.PrimaryRole == 0)
        {
            user.Candidate = new PTCandidate { SCUserId = user.Id, WizardStep = 0, WizardCompleted = false };
        }
        else if (dto.PrimaryRole == 1)
        {
            user.Company = new PTCompany { SCUserId = user.Id };
        }

        _context.SC_Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.SC_Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserPreference)
            .Include(u => u.Candidate)
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
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

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

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(string googleToken)
    {
        // In production, validate the Google ID token with Google's API
        // For now, we decode the JWT payload to extract the email and GoogleId
        // This is a simplified implementation - production should use Google's tokeninfo endpoint
        try
        {
            var parts = googleToken.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var email = doc.RootElement.GetProperty("email").GetString();
            var googleId = doc.RootElement.GetProperty("sub").GetString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId)) return null;

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
