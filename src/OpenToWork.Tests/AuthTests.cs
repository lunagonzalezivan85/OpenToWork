using System.Net;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Pruebas de autenticación: login, credenciales incorrectas, refresh, validaciones.
/// </summary>
public class AuthTests : BaseTest
{
    [Fact]
    public async Task Login_ConCredencialesValidas_RetornaTokenYUsuario()
    {
        var loginDto = new LoginDto
        {
            Email = "juan.perez@gmail.com",
            Password = "Candidato123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result!.Token));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.NotNull(result.User);
        Assert.Equal("juan.perez@gmail.com", result.User.Email);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_RetornaUnauthorized()
    {
        var loginDto = new LoginDto
        {
            Email = "juan.perez@gmail.com",
            Password = "PasswordIncorrecta123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConEmailInexistente_RetornaUnauthorized()
    {
        var loginDto = new LoginDto
        {
            Email = "noexiste@test.com",
            Password = "Candidato123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConEmailVacio_RetornaBadRequest()
    {
        var loginDto = new LoginDto
        {
            Email = "",
            Password = "Candidato123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConPasswordVacia_RetornaBadRequest()
    {
        var loginDto = new LoginDto
        {
            Email = "juan.perez@gmail.com",
            Password = "",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ConTokenValido_RetornaNuevoToken()
    {
        var loginDto = new LoginDto
        {
            Email = "juan.perez@gmail.com",
            Password = "Candidato123!",
            RememberMe = false
        };

        var loginResponse = await Client.PostAsJsonAsync("api/auth/login", loginDto);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);

        var refreshDto = new RefreshTokenDto { RefreshToken = auth!.RefreshToken };
        var refreshResponse = await Client.PostAsJsonAsync("api/auth/refresh", refreshDto);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var newAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(newAuth);
        Assert.False(string.IsNullOrEmpty(newAuth!.Token));
    }

    [Fact]
    public async Task CheckDevice_SinAutenticar_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("api/auth/check-device?deviceHash=test123");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConMariaGonzalez_RetornaTokenValido()
    {
        var loginDto = new LoginDto
        {
            Email = "maria.gonzalez@hotmail.com",
            Password = "Candidato123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result!.Token));
    }

    [Fact]
    public async Task Login_ConCarlosRodriguez_RetornaTokenValido()
    {
        var loginDto = new LoginDto
        {
            Email = "carlos.rodriguez@outlook.com",
            Password = "Candidato123!",
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result!.Token));
    }
}
