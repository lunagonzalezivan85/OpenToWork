namespace OpenToWork.Core.Interfaces;

/// <summary>Datos de un ID token de Google ya validado (firma, emisor, audiencia y vigencia).</summary>
public record GoogleIdentity(string Subject, string Email, bool EmailVerified);

public interface IGoogleTokenValidator
{
    /// <summary>true si el login con Google esta configurado (GoogleOAuth:ClientId).</summary>
    bool IsEnabled { get; }

    /// <summary>Valida el ID token contra las claves publicas de Google. Null si es invalido,
    /// esta vencido, no es para esta app o el login con Google no esta configurado.</summary>
    Task<GoogleIdentity?> ValidateAsync(string idToken);
}
