using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas de las referencias que aporta el candidato: la declaracion de que informo a la persona
/// de referencia y cuenta con su autorizacion es obligatoria.
/// </summary>
public class ReferencesTests : BaseTest
{
    private async Task<Guid> GetMyCandidateIdAsync()
    {
        var profile = await Client.GetFromJsonAsync<CandidateProfileDto>("api/profile");
        Assert.NotNull(profile);
        return profile!.Id;
    }

    [Fact]
    public async Task AddReference_SinDeclaracion_RetornaBadRequest()
    {
        await AuthenticateAsync();
        var candidateId = await GetMyCandidateIdAsync();

        var dto = new CreateReferenceDto
        {
            ContactName = "Referencia Sin Declaracion",
            CompanyName = "Empresa Prueba",
            ContactAuthorized = false
        };

        var response = await Client.PostAsJsonAsync($"api/candidates/{candidateId}/references", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddReference_ConDeclaracion_CreaLaReferencia()
    {
        await AuthenticateAsync();
        var candidateId = await GetMyCandidateIdAsync();

        var dto = new CreateReferenceDto
        {
            ContactName = "Referencia Con Declaracion",
            CompanyName = "Empresa Prueba",
            ContactAuthorized = true
        };

        var response = await Client.PostAsJsonAsync($"api/candidates/{candidateId}/references", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CandidateReferenceDto>();
        Assert.NotNull(created);
        Assert.Equal("Referencia Con Declaracion", created!.ContactName);
    }
}
