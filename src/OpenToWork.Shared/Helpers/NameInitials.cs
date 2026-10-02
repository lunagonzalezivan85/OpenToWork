namespace OpenToWork.Shared.Helpers;

/// <summary>
/// Iniciales del avatar de una persona: primer nombre + primer apellido (pedido de Darwin, 1-Oct).
/// "Laura María" + "Gómez Ruiz" da "LG", no "LM" como al partir el nombre completo por espacios.
/// </summary>
public static class NameInitials
{
    public static string From(string? firstName, string? lastName, string fallback = "U")
    {
        var first = FirstLetter(firstName);
        var last = FirstLetter(lastName);
        if (first == null && last == null) return fallback;
        return ((first ?? "") + (last ?? "")).ToUpperInvariant();
    }

    private static string? FirstLetter(string? value)
    {
        var word = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrEmpty(word) ? null : word[..1];
    }
}
