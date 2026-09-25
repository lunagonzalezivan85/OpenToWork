using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

/// <summary>
/// Valida de verdad el ID token de "Iniciar sesion con Google" (revision de seguridad 25-Sep): antes
/// solo se decodificaba el payload sin verificar la firma, y cualquiera podia fabricar un token con el
/// correo de otra persona y entrar a su cuenta. Ahora se exige firma de Google (claves publicas JWKS),
/// emisor de Google, audiencia = nuestro ClientId y vigencia. Sin ClientId configurado, el login con
/// Google queda desactivado.
/// </summary>
public class GoogleTokenValidator : IGoogleTokenValidator
{
    private const string JwksUrl = "https://www.googleapis.com/oauth2/v3/certs";
    private static readonly string[] ValidIssuers = { "accounts.google.com", "https://accounts.google.com" };
    private static readonly TimeSpan KeysCacheDuration = TimeSpan.FromHours(6);

    // Las claves de Google rotan cada varios dias: se cachean y se recargan si una firma no valida.
    private static readonly SemaphoreSlim KeysLock = new(1, 1);
    private static IList<SecurityKey>? _cachedKeys;
    private static DateTime _cachedAt;

    private readonly HttpClient _http;
    private readonly ILogger<GoogleTokenValidator> _logger;
    private readonly string? _clientId;

    public GoogleTokenValidator(HttpClient http, IConfiguration config, ILogger<GoogleTokenValidator> logger)
    {
        _http = http;
        _logger = logger;
        _clientId = config["GoogleOAuth:ClientId"];
    }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(_clientId);

    public async Task<GoogleIdentity?> ValidateAsync(string idToken)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(idToken)) return null;

        var identity = await TryValidateAsync(idToken, forceRefreshKeys: false);
        // Una firma que no valida puede deberse a que Google roto las claves: un reintento con claves nuevas.
        return identity ?? await TryValidateAsync(idToken, forceRefreshKeys: true);
    }

    private async Task<GoogleIdentity?> TryValidateAsync(string idToken, bool forceRefreshKeys)
    {
        var keys = await GetKeysAsync(forceRefreshKeys);
        if (keys.Count == 0) return null;

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidateIssuer = true,
            ValidIssuers = ValidIssuers,
            ValidateAudience = true,
            ValidAudience = _clientId,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(idToken, parameters, out _);

            var sub = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var emailVerified = string.Equals(principal.FindFirst("email_verified")?.Value, "true", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email)) return null;

            return new GoogleIdentity(sub, email, emailVerified);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            _logger.LogWarning("Token de Google rechazado: {Reason}", ex.Message);
            return null;
        }
    }

    private async Task<IList<SecurityKey>> GetKeysAsync(bool forceRefresh)
    {
        if (!forceRefresh && _cachedKeys != null && DateTime.UtcNow - _cachedAt < KeysCacheDuration)
            return _cachedKeys;

        await KeysLock.WaitAsync();
        try
        {
            if (!forceRefresh && _cachedKeys != null && DateTime.UtcNow - _cachedAt < KeysCacheDuration)
                return _cachedKeys;

            var json = await _http.GetStringAsync(JwksUrl);
            _cachedKeys = new JsonWebKeySet(json).GetSigningKeys();
            _cachedAt = DateTime.UtcNow;
            return _cachedKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron obtener las claves publicas de Google");
            return _cachedKeys ?? new List<SecurityKey>();
        }
        finally
        {
            KeysLock.Release();
        }
    }
}
