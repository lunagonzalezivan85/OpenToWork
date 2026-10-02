using OpenToWork.Shared.Enums;
using OpenToWork.Shared.Validation;

namespace OpenToWork.Tests;

/// <summary>
/// Validacion de DNI/NIE/pasaporte del registro de candidato. Pruebas unitarias puras: no necesitan
/// el portal ni la API levantados (a diferencia del resto de este proyecto).
/// </summary>
public class IdentityDocumentValidatorTests
{
    [Theory]
    [InlineData("12345678Z")]
    [InlineData("12345678z")]      // minuscula
    [InlineData("12.345.678-Z")]   // con separadores
    [InlineData(" 00000000T ")]    // espacios alrededor
    public void Dni_Valido(string value) =>
        Assert.True(IdentityDocumentValidator.IsValid(IdentityDocumentType.Dni, value));

    [Theory]
    [InlineData("12345678A")]      // letra de control incorrecta
    [InlineData("1234567Z")]       // 7 digitos
    [InlineData("123456789")]      // sin letra
    [InlineData("X1234567L")]      // es un NIE
    [InlineData("")]
    [InlineData(null)]
    public void Dni_Invalido(string? value) =>
        Assert.False(IdentityDocumentValidator.IsValid(IdentityDocumentType.Dni, value));

    [Theory]
    [InlineData("X1234567L")]
    [InlineData("Y1234567X")]
    [InlineData("Z1234567R")]
    [InlineData("x-1234567-l")]
    public void Nie_Valido(string value) =>
        Assert.True(IdentityDocumentValidator.IsValid(IdentityDocumentType.Nie, value));

    [Theory]
    [InlineData("X1234567A")]      // letra de control incorrecta
    [InlineData("A1234567L")]      // solo X, Y o Z al inicio
    [InlineData("X123456L")]       // 6 digitos
    [InlineData("12345678Z")]      // es un DNI
    public void Nie_Invalido(string value) =>
        Assert.False(IdentityDocumentValidator.IsValid(IdentityDocumentType.Nie, value));

    [Theory]
    [InlineData("PAA123456")]
    [InlineData("123456789")]
    [InlineData("ab12cd")]
    public void Pasaporte_Valido(string value) =>
        Assert.True(IdentityDocumentValidator.IsValid(IdentityDocumentType.Passport, value));

    [Theory]
    [InlineData("AB12")]           // menos de 5
    [InlineData("AB12345678901234567890")] // mas de 20
    [InlineData("AB12#456")]       // simbolos
    [InlineData("ÑA123456")]       // letra fuera de A-Z
    public void Pasaporte_Invalido(string value) =>
        Assert.False(IdentityDocumentValidator.IsValid(IdentityDocumentType.Passport, value));

    [Theory]
    [InlineData("12345678Z")]      // autonomo con DNI
    [InlineData("X1234567L")]      // autonomo con NIE
    [InlineData("B12345674")]      // S.L.: control obligatoriamente digito
    [InlineData("A58818501")]      // S.A.
    [InlineData("Q2826000H")]      // organismo publico: control obligatoriamente letra
    [InlineData("G1234567D")]      // asociacion: acepta letra...
    [InlineData("G12345674")]      // ...o digito
    [InlineData("b-12345674")]     // minuscula y guion
    public void NifEmpresa_Valido(string value) =>
        Assert.True(IdentityDocumentValidator.IsValidCompanyNif(value));

    [Theory]
    [InlineData("B12345675")]      // control incorrecto
    [InlineData("B1234567D")]      // S.L. con letra de control (debe ser digito)
    [InlineData("Q28260008")]      // organismo con digito (debe ser letra)
    [InlineData("I12345674")]      // I no es un tipo de entidad
    [InlineData("12345678A")]      // DNI con letra incorrecta
    [InlineData("B123456")]        // corto
    [InlineData("")]
    [InlineData(null)]
    public void NifEmpresa_Invalido(string? value) =>
        Assert.False(IdentityDocumentValidator.IsValidCompanyNif(value));

    [Fact]
    public void Normalize_QuitaSeparadoresYPasaAMayusculas() =>
        Assert.Equal("X1234567L", IdentityDocumentValidator.Normalize(" x.1234567-l "));
}
