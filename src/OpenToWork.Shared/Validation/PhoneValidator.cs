using System.Text.RegularExpressions;

namespace OpenToWork.Shared.Validation;

/// <summary>
/// Telefono de contacto del registro. Formato abierto (empresas con numeros de fuera de Espana):
/// "+" opcional al inicio y entre 9 y 15 digitos, admitiendo espacios, guiones, puntos y parentesis.
/// </summary>
public static class PhoneValidator
{
    /// <summary>Quita espacios, guiones, puntos y parentesis: "+34 600-00 00 00" queda "+34600000000".</summary>
    public static string Normalize(string? value) =>
        Regex.Replace(value ?? string.Empty, @"[\s\-\.\(\)]", string.Empty);

    public static bool IsValid(string? value) =>
        Regex.IsMatch(Normalize(value), @"^\+?\d{9,15}$");
}
