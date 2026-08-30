using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using OpenToWork.SharedUI.Services;

namespace OpenToWork.AdminWEB.Services;

public class AdminAuthStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorage;

    public AdminAuthStateProvider(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync("otwadmin-token");

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
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

        return keyValuePairs.Select(kvp =>
        {
            var key = kvp.Key;
            if (key == "sub") key = ClaimTypes.NameIdentifier;
            if (key == "email") key = ClaimTypes.Email;
            if (key == "role" || key == "primaryRole") key = ClaimTypes.Role;
            return new Claim(key, kvp.Value.ToString() ?? string.Empty);
        });
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
