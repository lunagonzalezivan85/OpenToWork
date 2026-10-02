using OpenToWork.Core.Services;

namespace OpenToWork.Tests;

/// <summary>Correo del codigo de verificacion. Pruebas unitarias puras (sin servidor).</summary>
public class EmailTemplatesTests
{
    [Fact]
    public void VerificationCode_IncluyeCodigoVencimientoYSaludo()
    {
        var html = EmailTemplates.VerificationCode("Marta", "482913", 15);

        Assert.Contains(">482913<", html);
        Assert.Contains("Tu código es 482913. Vence en 15 minutos.", html); // texto de vista previa
        Assert.Contains("15 minutos", html);
        Assert.Contains("Hola, Marta", html);
    }

    [Fact]
    public void VerificationCode_SinNombre_SaludaSinComa()
    {
        var html = EmailTemplates.VerificationCode(null, "000001", 15);

        Assert.Contains("Hola, estás a un paso", html);
    }

    [Fact]
    public void VerificationCode_EscapaElNombre()
    {
        var html = EmailTemplates.VerificationCode("<script>alert(1)</script>", "123456", 15);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }
}
