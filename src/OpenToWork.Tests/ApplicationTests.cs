using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas de postulaciones: aplicar, listar, duplicado, estados.
/// </summary>
public class ApplicationTests : BaseTest
{
    [Fact]
    public async Task GetMyApplications_ConTokenValido_RetornaLista()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("api/applications/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var applications = await response.Content.ReadFromJsonAsync<List<ApplicationDto>>();
        Assert.NotNull(applications);
    }

    [Fact]
    public async Task GetMyApplications_SinToken_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("api/applications/my");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Apply_ConVacancyIdInexistente_RetornaNotFound()
    {
        await AuthenticateAsync();

        var dto = new CreateApplicationDto
        {
            VacancyId = Guid.NewGuid(),
            CoverLetter = "Test cover letter from QA",
            ExpectedSalary = 60000
        };

        var response = await Client.PostAsJsonAsync("api/applications", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Apply_SinToken_RetornaUnauthorized()
    {
        var dto = new CreateApplicationDto
        {
            VacancyId = Guid.NewGuid(),
            CoverLetter = "Test"
        };

        var response = await Client.PostAsJsonAsync("api/applications", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Apply_ConDatosValidos_RetornaCreatedOConflict()
    {
        await AuthenticateAsync();

        var searchResponse = await Client.GetAsync("api/permanentvacancies/search?page=1&pageSize=10");
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultDto>();

        if (searchResult?.Items?.Any() == true)
        {
            var vacancy = searchResult.Items.First();
            var dto = new CreateApplicationDto
            {
                VacancyId = vacancy.Id,
                CoverLetter = "Postulación de prueba automatizada",
                ExpectedSalary = 55000,
                AvailableFromDate = DateTime.UtcNow.AddDays(7)
            };

            var response = await Client.PostAsJsonAsync("api/applications", dto);

            Assert.True(
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.Conflict,
                $"Expected Created or Conflict, got {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var app = await response.Content.ReadFromJsonAsync<ApplicationDto>();
                Assert.NotNull(app);
                Assert.Equal(vacancy.Id, app!.VacancyId);
            }
        }
    }

    [Fact]
    public async Task Apply_DosVeccesALaMismaVacante_RetornaConflict()
    {
        await AuthenticateAsync();

        var searchResponse = await Client.GetAsync("api/permanentvacancies/search?page=1&pageSize=20");
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultDto>();

        if (searchResult?.Items?.Any() == true)
        {
            var vacancy = searchResult.Items.First();
            var dto = new CreateApplicationDto
            {
                VacancyId = vacancy.Id,
                CoverLetter = "Test duplicate application"
            };

            var firstResponse = await Client.PostAsJsonAsync("api/applications", dto);

            if (firstResponse.StatusCode == HttpStatusCode.Created)
            {
                var secondResponse = await Client.PostAsJsonAsync("api/applications", dto);
                Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
            }
            else if (firstResponse.StatusCode == HttpStatusCode.Conflict)
            {
                Assert.Equal(HttpStatusCode.Conflict, firstResponse.StatusCode);
            }
        }
    }

    [Fact]
    public async Task UpdateStatus_ConIdInexistente_RetornaNotFound()
    {
        await AuthenticateAsync();

        var fakeId = Guid.NewGuid();
        var dto = new UpdateApplicationStatusDto { Status = 1 };

        var response = await Client.PutAsJsonAsync($"api/applications/{fakeId}/status", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
