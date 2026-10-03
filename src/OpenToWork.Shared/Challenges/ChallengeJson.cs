using System.Text.Json;

namespace OpenToWork.Shared.Challenges;

/// <summary>Serializacion unica de definiciones, respuestas y resultados de retos (BD y copias).</summary>
public static class ChallengeJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        // Tipos como texto ("Cards", "Ordering"): legible en BD y en el contenido inicial; acepta numeros.
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Deserialize<T>(string? json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json)) return new T();
        try { return JsonSerializer.Deserialize<T>(json, Options) ?? new T(); }
        catch (JsonException) { return new T(); }
    }

    /// <summary>Copia profunda (para duplicar retos y versiones sin compartir referencias).</summary>
    public static T Clone<T>(T value) where T : new() => Deserialize<T>(Serialize(value));
}
