using OpenToWork.Shared.Validation;

namespace OpenToWork.Tests;

/// <summary>Telefono de contacto del registro de empresa. Pruebas unitarias puras (sin servidor).</summary>
public class PhoneValidatorTests
{
    [Theory]
    [InlineData("600000000")]
    [InlineData("+34 600 00 00 00")]
    [InlineData("(+34) 600-000-000")]
    [InlineData("912.345.678")]
    [InlineData("+51987654321")]       // numero de fuera de Espana
    public void Telefono_Valido(string value) => Assert.True(PhoneValidator.IsValid(value));

    [Theory]
    [InlineData("60000000")]           // 8 digitos
    [InlineData("6000000000000000")]   // 16 digitos
    [InlineData("600-ABC-000")]
    [InlineData("34+600000000")]       // + en medio
    [InlineData("")]
    [InlineData(null)]
    public void Telefono_Invalido(string? value) => Assert.False(PhoneValidator.IsValid(value));

    [Fact]
    public void Normalize_QuitaSeparadores() =>
        Assert.Equal("+34600000000", PhoneValidator.Normalize("+34 600-00.00 (00)"));
}
