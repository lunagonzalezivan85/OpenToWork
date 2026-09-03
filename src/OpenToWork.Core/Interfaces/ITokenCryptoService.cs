using System.Security.Claims;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Shared JWT/refresh-token cryptography used by both AuthService (portal principal)
/// and AdminAuthService (portal admin). Each caller keeps its own claims-building logic
/// and its own Jwt:Key/Issuer/Audience configuration values - only the crypto primitives
/// are shared here.
/// </summary>
public interface ITokenCryptoService
{
    string CreateJwtToken(IEnumerable<Claim> claims, string signingKey, string issuer, string audience, int expireMinutes);
    string GenerateRefreshToken();
    string HashToken(string token);
}
