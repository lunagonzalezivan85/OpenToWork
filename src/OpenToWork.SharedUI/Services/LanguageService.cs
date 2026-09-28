using System.Text.Json;
using Microsoft.JSInterop;

namespace OpenToWork.SharedUI.Services;

/// <summary>
/// Loads flat-file JSON translations. Server apps (AdminWEB) read from wwwroot via
/// <see cref="IWebHostEnvironment"/>; WASM (portal candidato) fetches the same
/// config/language/*.json over HTTP. Each entry in <paramref name="sections"/>
/// is both the JSON filename and the flatten prefix for its keys (e.g. "common.save").
/// </summary>
public class LanguageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string? _webRootPath;
    private readonly HttpClient? _http;
    private readonly string[] _sections;
    private string _currentLanguage = "es";
    public Dictionary<string, string> _translations = new();

    public event Action? OnLanguageChanged;

    /// <summary>Blazor Server: lee los JSON del filesystem (wwwroot).</summary>
    public LanguageService(IJSRuntime jsRuntime, string webRootPath, string[] sections)
    {
        _jsRuntime = jsRuntime;
        _webRootPath = webRootPath;
        _sections = sections;
    }

    /// <summary>Blazor WebAssembly: descarga los JSON via HTTP desde el origen de la app.</summary>
    public LanguageService(IJSRuntime jsRuntime, HttpClient http, string[] sections)
    {
        _jsRuntime = jsRuntime;
        _http = http;
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
        // Espanol por defecto; un valor guardado que no sea un idioma soportado tambien cae a espanol
        _currentLanguage = saved is "es" or "en" ? saved : "es";
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
        // Todas las secciones en paralelo, y el diccionario se sustituye de una vez al final: nunca queda
        // vacio ni a medias mientras se renderiza (antes se vaciaba y se veian las claves, p. ej. "common.home.heroTitle").
        var jsons = await Task.WhenAll(_sections.Select(async section =>
        {
            try { return (section, json: await LoadSectionJsonAsync(lang, section)); }
            catch { return (section, json: (string?)null); }
        }));

        var loaded = new Dictionary<string, string>();
        foreach (var (section, json) in jsons)
        {
            if (json == null) continue;
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null) FlattenDictionary(dict, section, loaded);
            }
            catch { }
        }
        _translations = loaded;
    }

    private async Task<string?> LoadSectionJsonAsync(string lang, string section)
    {
        if (_http != null)
        {
            var response = await _http.GetAsync($"config/language/{lang}/{section}.json");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }

        var filePath = Path.Combine(_webRootPath!, "config", "language", lang, $"{section}.json");
        return File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : null;
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
