using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ITokenCryptoService _tokenCrypto;

    public AdminAuthService(AppDbContext context, IConfiguration config, ITokenCryptoService tokenCrypto)
    {
        _context = context;
        _config = config;
        _tokenCrypto = tokenCrypto;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.SC_Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.PrimaryRole != (int)UserRole.Admin)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Password expired");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(SCUser user)
    {
        var token = GenerateJwtToken(user);
        var refreshToken = _tokenCrypto.GenerateRefreshToken();

        var expireDays = _config.GetValue<int>("Jwt:RefreshTokenExpireDays", 1);

        _context.SC_RefreshTokens.Add(new SCRefreshToken
        {
            SCUserId = user.Id,
            TokenHash = _tokenCrypto.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
            IsRevoked = false
        });
        await _context.SaveChangesAsync();

        var expireMinutes = _config.GetValue<int>("Jwt:ExpireMinutes", 60);

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
                StaffRole = user.StaffRole,
                EmailVerified = user.EmailVerified,
                IsActive = user.IsActive,
                Roles = user.UserRoles.Select(r => r.Role).ToList()
            }
        };
    }

    private string GenerateJwtToken(SCUser user)
    {
        var expireMinutes = _config.GetValue<int>("Jwt:ExpireMinutes", 60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("primaryRole", user.PrimaryRole.ToString()),
            new("staffRole", user.StaffRole?.ToString() ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, UserRole.Admin.ToString())
        };

        return _tokenCrypto.CreateJwtToken(claims, _config["Jwt:Key"]!, _config["Jwt:Issuer"]!, _config["Jwt:Audience"]!, expireMinutes);
    }
}
