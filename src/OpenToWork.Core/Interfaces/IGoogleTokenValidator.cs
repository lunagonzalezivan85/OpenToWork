namespace OpenToWork.Core.Interfaces;

/// <summary>Datos de un ID token de Google ya validado (firma, emisor, audiencia y vigencia).</summary>
public record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? GivenName = null, string? FamilyName = null);

public interface IGoogleTokenValidator
{
    /// <summary>true si el login con Google esta configurado (GoogleOAuth:ClientId y ClientSecret).</summary>
    bool IsEnabled { get; }

    /// <summary>Valida el ID token contra las claves publicas de Google. Null si es invalido,
    /// esta vencido, no es para esta app o el login con Google no esta configurado.</summary>
    Task<GoogleIdentity?> ValidateAsync(string idToken);

    /// <summary>URL de la pagina de Google a la que se manda al candidato (flujo por redireccion).</summary>
    string BuildAuthorizeUrl(string redirectUri, string state);

    /// <summary>Cambia el codigo que Google devuelve al callback por el ID token y lo valida. Null si falla.</summary>
    Task<GoogleIdentity?> ExchangeCodeAsync(string code, string redirectUri);
}
