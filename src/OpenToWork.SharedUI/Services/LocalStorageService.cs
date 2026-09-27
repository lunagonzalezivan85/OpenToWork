using Microsoft.JSInterop;

namespace OpenToWork.SharedUI.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SecureValueCipher _cipher;

    public LocalStorageService(IJSRuntime jsRuntime, SecureValueCipher cipher)
    {
        _jsRuntime = jsRuntime;
        _cipher = cipher;
    }

    /// <summary>Claves cuyo valor viaja cifrado a localStorage (tokens, credenciales).</summary>
    private static bool IsSensitiveKey(string key) =>
        key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("password", StringComparison.OrdinalIgnoreCase);

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            var value = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            return IsSensitiveKey(key) ? _cipher.Decrypt(value) : value;
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, IsSensitiveKey(key) ? _cipher.Encrypt(value) : value);
        }
        catch (JSDisconnectedException) { }
        catch (InvalidOperationException) { }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSDisconnectedException) { }
        catch (InvalidOperationException) { }
    }
}
