using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Helpers;

namespace OpenToWork.Tests;

/// <summary>Porcentaje/pendientes del perfil e iniciales del avatar. Pruebas unitarias puras (sin servidor).</summary>
public class ProfileHelpersTests
{
    [Theory]
    [InlineData("Laura María", "Gómez Ruiz", "LG")]   // primer nombre + primer apellido, no "LM"
    [InlineData("marta", "lópez vidal", "ML")]
    [InlineData("Ana", "", "A")]
    [InlineData("", "", "U")]
    [InlineData(null, null, "U")]
    public void Iniciales_PrimerNombreYPrimerApellido(string? first, string? last, string expected) =>
        Assert.Equal(expected, NameInitials.From(first, last));

    [Fact]
    public void Perfil_Vacio_SoloConNombre()
    {
        var c = new CandidateDto { FirstName = "Ana", LastName = "Paz" };

        var missing = ProfileCompletion.Missing(c);

        Assert.DoesNotContain(ProfileCompletion.FirstName, missing);
        Assert.Contains(ProfileCompletion.Cv, missing);
        Assert.Equal(12, missing.Count);
        Assert.Equal(14, ProfileCompletion.Percent(c)); // 2 de 14 datos
    }

    [Fact]
    public void Perfil_Completo_Es100_SinDependerDelAsistente()
    {
        var c = new CandidateDto
        {
            FirstName = "Ana", LastName = "Paz", Identification = "12345678Z", Phone = "600000000",
            BirthDate = new DateTime(1990, 1, 1), Title = "Camarera", Summary = "Resumen",
            Country = "España", City = "Madrid", YearsOfExperience = 5, LinkedInUrl = "https://linkedin.com/in/ana",
            CvUrl = "cv.pdf", WizardCompleted = false,
            Experiences = { new CandidateExperienceDto() },
            Educations = { new CandidateEducationDto() }
        };

        Assert.Empty(ProfileCompletion.Missing(c));
        Assert.Equal(100, ProfileCompletion.Percent(c));
    }
}
