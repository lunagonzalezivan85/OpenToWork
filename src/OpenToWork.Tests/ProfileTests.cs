using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas del perfil del candidato: obtener, actualizar, experiencia, educación, certificaciones.
/// </summary>
public class ProfileTests : BaseTest
{
    [Fact]
    public async Task GetProfile_ConTokenValido_RetornaPerfil()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("api/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
        Assert.NotNull(profile);
        Assert.False(string.IsNullOrEmpty(profile!.FirstName));
    }

    [Fact]
    public async Task GetProfile_SinToken_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ConDatosValidos_RetornaPerfilActualizado()
    {
        await AuthenticateAsync();

        var getResponse = await Client.GetAsync("api/profile");
        var profile = await getResponse.Content.ReadFromJsonAsync<CandidateProfileDto>();
        Assert.NotNull(profile);

        var updateDto = new UpdateCandidateProfileDto
        {
            Title = "Desarrollador Full Stack Senior",
            Summary = profile!.Summary ?? "Profesional con experiencia en desarrollo web.",
            WorkAuthorization = profile.WorkAuthorization,
            IsProfilePublic = profile.IsProfilePublic,
            CvUrl = profile.CvUrl
        };

        var response = await Client.PutAsJsonAsync("api/profile", updateDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
        Assert.NotNull(updated);
        Assert.Equal("Desarrollador Full Stack Senior", updated!.Title);
    }

    [Fact]
    public async Task AddExperience_ConDatosValidos_RetornaExperienciaCreada()
    {
        await AuthenticateAsync();

        var newExp = new CreateExperienceDto
        {
            CompanyName = "Empresa Test QA",
            JobTitle = "QA Engineer",
            Description = "Pruebas automatizadas y manuales",
            StartDate = new DateTime(2022, 1, 1),
            EndDate = new DateTime(2023, 12, 31),
            IsCurrentJob = false
        };

        var response = await Client.PostAsJsonAsync("api/profile/experience", newExp);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CandidateExperienceDto>();
        Assert.NotNull(result);
        Assert.Equal("Empresa Test QA", result!.CompanyName);
        Assert.Equal("QA Engineer", result.JobTitle);
    }

    [Fact]
    public async Task AddExperience_SinCompanyName_RetornaBadRequest()
    {
        await AuthenticateAsync();

        var newExp = new CreateExperienceDto
        {
            CompanyName = "",
            JobTitle = "QA Engineer",
            StartDate = new DateTime(2022, 1, 1)
        };

        var response = await Client.PostAsJsonAsync("api/profile/experience", newExp);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddEducation_ConDatosValidos_RetornaEducacionCreada()
    {
        await AuthenticateAsync();

        var newEdu = new CreateEducationDto
        {
            Institution = "Universidad Test QA",
            Degree = "Licenciatura en Informática",
            FieldOfStudy = "Ingeniería de Software",
            StartDate = new DateTime(2018, 1, 1),
            EndDate = new DateTime(2022, 12, 31)
        };

        var response = await Client.PostAsJsonAsync("api/profile/education", newEdu);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CandidateEducationDto>();
        Assert.NotNull(result);
        Assert.Equal("Universidad Test QA", result!.Institution);
    }

    [Fact]
    public async Task AddCertification_ConDatosValidos_RetornaCertificacionCreada()
    {
        await AuthenticateAsync();

        var newCert = new CreateCertificationDto
        {
            Name = "Azure Fundamentals AZ-900",
            Issuer = "Microsoft",
            IssueDate = new DateTime(2023, 6, 15),
            ExpiryDate = null,
            CredentialId = "QA-AZ900-001",
            CredentialUrl = "https://learn.microsoft.com/cert"
        };

        var response = await Client.PostAsJsonAsync("api/profile/certification", newCert);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CandidateCertificationDto>();
        Assert.NotNull(result);
        Assert.Equal("Azure Fundamentals AZ-900", result!.Name);
        Assert.Equal("Microsoft", result.Issuer);
    }

    [Fact]
    public async Task DeleteExperience_ConIdInexistente_RetornaNotFound()
    {
        await AuthenticateAsync();

        var fakeId = Guid.NewGuid();
        var response = await Client.DeleteAsync($"api/profile/experience/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEducation_ConIdInexistente_RetornaNotFound()
    {
        await AuthenticateAsync();

        var fakeId = Guid.NewGuid();
        var response = await Client.DeleteAsync($"api/profile/education/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
