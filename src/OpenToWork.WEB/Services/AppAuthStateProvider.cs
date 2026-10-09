using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using OpenToWork.Shared.DTOs;
using OpenToWork.SharedUI.Services;

namespace OpenToWork.WEB.Services;

public class AppAuthStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorage;
    private readonly ApiAuthService _apiAuth;

    public AppAuthStateProvider(LocalStorageService localStorage, ApiAuthService apiAuth)
    {
        _localStorage = localStorage;
        _apiAuth = apiAuth;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync("opentowork-token");

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token).ToList();

        // Un token caducado (o malformed) no autentica: sin esto el guard de rutas dejaba
        // pasar a las paginas privadas con una sesion muerta (auditoria 08-Oct H-29).
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (!long.TryParse(exp, out var expUnix) ||
            DateTimeOffset.FromUnixTimeSeconds(expUnix) <= DateTimeOffset.UtcNow)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new();

        var claims = new List<Claim>();
        foreach (var kvp in keyValuePairs)
        {
            var key = kvp.Key;
            if (key == "sub") key = ClaimTypes.NameIdentifier;
            if (key == "email") key = ClaimTypes.Email;
            if (key == "role") key = ClaimTypes.Role;
            // primaryRole se queda con su nombre (es numerico, no un nombre de rol).

            // Un usuario puede tener varios roles: el JWT los serializa como array JSON y hay
            // que expandirlos a claims individuales o IsInRole("Company") nunca coincide.
            if (kvp.Value is System.Text.Json.JsonElement el && el.ValueKind == System.Text.Json.JsonValueKind.Array)
                claims.AddRange(el.EnumerateArray().Select(v => new Claim(key, v.ToString())));
            else
                claims.Add(new Claim(key, kvp.Value?.ToString() ?? string.Empty));
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
