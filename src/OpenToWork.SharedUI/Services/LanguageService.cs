using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.JSInterop;

namespace OpenToWork.SharedUI.Services;

/// <summary>
/// Loads flat-file JSON translations for a Blazor Server app. Each entry in <paramref name="sections"/>
/// (passed at registration time) is both the JSON filename (wwwroot/config/language/{lang}/{section}.json)
/// and the flatten prefix for its keys (e.g. "common.save"). AdminWEB registers a single "admin" section;
/// WEB registers its full set of portal sections - this preserves both apps' existing behavior exactly.
/// </summary>
public class LanguageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IWebHostEnvironment _env;
    private readonly string[] _sections;
    private string _currentLanguage = "es";
    public Dictionary<string, string> _translations = new();

    public event Action? OnLanguageChanged;

    public LanguageService(IJSRuntime jsRuntime, IWebHostEnvironment env, string[] sections)
    {
        _jsRuntime = jsRuntime;
        _env = env;
        _sections = sections;
    }

    public string CurrentLanguage => _currentLanguage;

    public async Task InitializeAsync()
    {
        string? saved = null;
        try
        {
            saved = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "opentowork-lang");
        }
        catch (JSDisconnectedException) { }
        catch (InvalidOperationException) { }
        _currentLanguage = saved ?? "es";
        await LoadTranslationsAsync(_currentLanguage);
    }

    public async Task SetLanguageAsync(string lang)
    {
        _currentLanguage = lang;
        await LoadTranslationsAsync(lang);
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "opentowork-lang", lang);
        }
        catch (JSDisconnectedException) { }
        catch (InvalidOperationException) { }
        OnLanguageChanged?.Invoke();
    }

    public async Task LoadTranslationsAsync(string lang)
    {
        _translations.Clear();
        var basePath = Path.Combine(_env.WebRootPath, "config", "language", lang);
        foreach (var section in _sections)
        {
            try
            {
                var filePath = Path.Combine(basePath, $"{section}.json");
                if (!File.Exists(filePath)) continue;
                var json = await File.ReadAllTextAsync(filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null) FlattenDictionary(dict, section, _translations);
            }
            catch { }
        }
    }

    public string T(string key) => _translations.TryGetValue(key, out var value) ? value : key;

    private static void FlattenDictionary(Dictionary<string, object> dict, string prefix, Dictionary<string, string> result)
    {
        foreach (var kvp in dict)
        {
            var fullKey = $"{prefix}.{kvp.Key}";
            if (kvp.Value is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                var nested = je.Deserialize<Dictionary<string, object>>();
                if (nested != null) FlattenDictionary(nested, fullKey, result);
            }
            else
            {
                result[fullKey] = kvp.Value?.ToString() ?? fullKey;
            }
        }
    }
}
