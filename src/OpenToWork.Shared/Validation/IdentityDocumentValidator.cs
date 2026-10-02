using System.Text.RegularExpressions;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Shared.Validation;

/// <summary>
/// Validacion de DNI, NIE y pasaporte. Vive en Shared para que el portal (aviso inmediato en el
/// formulario) y la API (validacion que manda) apliquen exactamente la misma regla.
/// </summary>
public static class IdentityDocumentValidator
{
    private const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    /// <summary>Mayusculas y sin espacios, guiones ni puntos: "12.345.678-z" queda "12345678Z".</summary>
    public static string Normalize(string? value) =>
        Regex.Replace(value ?? string.Empty, @"[\s\-\.]", string.Empty).ToUpperInvariant();

    public static bool IsValid(IdentityDocumentType type, string? value) => type switch
    {
        IdentityDocumentType.Dni => IsValidDni(value),
        IdentityDocumentType.Nie => IsValidNie(value),
        IdentityDocumentType.Passport => IsValidPassport(value),
        _ => false
    };

    /// <summary>8 digitos + letra de control.</summary>
    public static bool IsValidDni(string? value)
    {
        var dni = Normalize(value);
        if (!Regex.IsMatch(dni, @"^\d{8}[A-Z]$")) return false;
        return dni[8] == ControlLetters[int.Parse(dni[..8]) % 23];
    }

    /// <summary>X, Y o Z + 7 digitos + letra de control (la X/Y/Z cuenta como 0/1/2 para el calculo).</summary>
    public static bool IsValidNie(string? value)
    {
        var nie = Normalize(value);
        if (!Regex.IsMatch(nie, @"^[XYZ]\d{7}[A-Z]$")) return false;
        var prefix = nie[0] switch { 'X' => '0', 'Y' => '1', _ => '2' };
        var number = int.Parse(prefix + nie.Substring(1, 7));
        return nie[8] == ControlLetters[number % 23];
    }

    /// <summary>Letras y numeros; el formato cambia segun el pais, asi que solo se acota la longitud.</summary>
    public static bool IsValidPassport(string? value) =>
        Regex.IsMatch(Normalize(value), @"^[A-Z0-9]{5,20}$");
}
