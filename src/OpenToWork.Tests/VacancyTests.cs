using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas de vacantes: búsqueda, detalle, crear, publicar, cerrar, eliminar.
/// </summary>
public class VacancyTests : BaseTest
{
    [Fact]
    public async Task Search_Vacantes_RetornaListaYTotal()
    {
        var response = await Client.GetAsync("api/permanentvacancies/search?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("items", content);
        Assert.Contains("total", content);
    }

    [Fact]
    public async Task Search_ConFiltroTexto_RetornaResultadosFiltrados()
    {
        var response = await Client.GetAsync("api/permanentvacancies/search?query=desarrollador&page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("items", content);
    }

    [Fact]
    public async Task Search_ConPaginaGrande_RetornaResultados()
    {
        var response = await Client.GetAsync("api/permanentvacancies/search?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ConIdInexistente_RetornaNotFound()
    {
        var fakeId = Guid.NewGuid();
        var response = await Client.GetAsync($"api/permanentvacancies/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ConIdValido_RetornaVacante()
    {
        var searchResponse = await Client.GetAsync("api/permanentvacancies/search?page=1&pageSize=1");
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultDto>();

        if (searchResult?.Items?.Any() == true)
        {
            var vacancyId = searchResult.Items.First().Id;
            var response = await Client.GetAsync($"api/permanentvacancies/{vacancyId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var vacancy = await response.Content.ReadFromJsonAsync<VacancyDto>();
            Assert.NotNull(vacancy);
            Assert.Equal(vacancyId, vacancy!.Id);
        }
    }

    [Fact]
    public async Task GetMyCompanyVacancies_SinToken_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("api/permanentvacancies/my-company");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_SinToken_RetornaUnauthorized()
    {
        var newVacancy = new CreateVacancyDto
        {
            Title = "Test QA Vacancy",
            Description = "Descripción de prueba",
            Requirements = "Requisitos de prueba",
            Location = "Remoto",
            ContractType = 0,
            WorkMode = 2,
            Category = "Tecnología"
        };

        var response = await Client.PostAsJsonAsync("api/permanentvacancies", newVacancy);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ConTokenValido_RetornaCreatedOBadRequest()
    {
        await AuthenticateAsync();

        var newVacancy = new CreateVacancyDto
        {
            Title = "QA Test Vacancy " + DateTime.Now.Ticks,
            Description = "Vacante creada por tests automatizados",
            Requirements = "Conocimientos de testing, xUnit, .NET",
            Location = "Remoto",
            ContractType = 0,
            WorkMode = 2,
            Category = "Tecnología",
            SalaryMin = 50000,
            SalaryMax = 80000
        };

        var response = await Client.PostAsJsonAsync("api/permanentvacancies", newVacancy);

        Assert.True(
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Created or BadRequest, got {response.StatusCode}");
    }

    [Fact]
    public async Task Create_ConTituloVacio_RetornaBadRequest()
    {
        await AuthenticateAsync();

        var newVacancy = new CreateVacancyDto
        {
            Title = "",
            Description = "Test",
            ContractType = 0,
            WorkMode = 0
        };

        var response = await Client.PostAsJsonAsync("api/permanentvacancies", newVacancy);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>
/// Helper DTO for parsing search results.
/// </summary>
public class SearchResultDto
{
    public List<VacancyDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
