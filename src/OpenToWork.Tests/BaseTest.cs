using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Tests;

/// <summary>
/// Helper base for integration tests. Each test class creates its own authenticated HttpClient.
/// </summary>
public abstract class BaseTest : IDisposable
{
    protected HttpClient Client { get; }
    protected string Token { get; private set; } = "";

    private const string BaseUrl = "http://localhost:5100";
    private const string TestEmail = "juan.perez@gmail.com";
    private const string TestPassword = "Candidato123!";

    protected BaseTest()
    {
        Client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    protected async Task AuthenticateAsync()
    {
        if (!string.IsNullOrEmpty(Token)) return;

        var loginDto = new LoginDto
        {
            Email = TestEmail,
            Password = TestPassword,
            RememberMe = false
        };

        var response = await Client.PostAsJsonAsync("api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth!.Token));

        Token = auth.Token;
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
    }

    protected void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
