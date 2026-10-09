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

    /// <summary>
    /// true si la forma es la de ese documento aunque la letra de control no cuadre. Sirve para avisar
    /// "la letra no corresponde a ese numero" en lugar de repetir el formato.
    /// </summary>
    public static bool HasValidFormat(IdentityDocumentType type, string? value) => type switch
    {
        IdentityDocumentType.Dni => Regex.IsMatch(Normalize(value), @"^\d{8}[A-Z]$"),
        IdentityDocumentType.Nie => Regex.IsMatch(Normalize(value), @"^[XYZ]\d{7}[A-Z]$"),
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

    /// <summary>
    /// NIF de una empresa en el registro. Se aceptan los tres formatos reales: autonomo con DNI
    /// (8 digitos + letra), autonomo extranjero con NIE, y sociedad (letra + 7 digitos + control,
    /// el antiguo CIF: una S.L. tiene B12345674, no 8 digitos + letra).
    /// </summary>
    public static bool IsValidCompanyNif(string? value) =>
        IsValidDni(value) || IsValidNie(value) || IsValidEntityNif(value);

    /// <summary>NIF de persona juridica (antiguo CIF): letra de tipo de entidad + 7 digitos + digito o letra de control.</summary>
    public static bool IsValidEntityNif(string? value)
    {
        var nif = Normalize(value);
        if (!Regex.IsMatch(nif, @"^[ABCDEFGHJNPQRSUVW]\d{7}[0-9A-J]$")) return false;

        // Digitos en posicion impar (1a, 3a, 5a, 7a) se doblan y se suman sus cifras; los pares se suman tal cual.
        var sum = 0;
        for (var i = 0; i < 7; i++)
        {
            var d = nif[i + 1] - '0';
            sum += i % 2 == 0 ? (d * 2 / 10) + (d * 2 % 10) : d;
        }
        var control = (10 - sum % 10) % 10;
        var digit = (char)('0' + control);
        var letter = "JABCDEFGHI"[control];

        // Segun el tipo de entidad el control es obligatoriamente letra (P, Q, R, S, N, W),
        // obligatoriamente digito (A, B, E, H) o cualquiera de los dos (el resto).
        var last = nif[8];
        return nif[0] switch
        {
            'P' or 'Q' or 'R' or 'S' or 'N' or 'W' => last == letter,
            'A' or 'B' or 'E' or 'H' => last == digit,
            _ => last == digit || last == letter
        };
    }
}
