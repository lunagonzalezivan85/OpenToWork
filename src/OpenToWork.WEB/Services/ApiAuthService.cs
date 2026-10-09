using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;
using OpenToWork.SharedUI.Services;

namespace OpenToWork.WEB.Services;

public class RegisterResult
{
    public AuthResponseDto? Data { get; set; }
    public bool EmailAlreadyRegistered { get; set; }
    /// <summary>Motivo del rechazo de la API (400) cuando un dato no es valido.</summary>
    public string? ValidationError { get; set; }
    /// <summary>Registro con Google: el enlace de Google vencio (hay que volver a pulsar el boton).</summary>
    public bool TicketExpired { get; set; }
}

/// <summary>Resultado de verificar o reenviar el codigo del correo. Error = clave corta de la API (invalid, expired...).</summary>
public record EmailCodeResult(bool Success, string? Error);

public record PublishVacancyResult(bool Success, string? Error);

/// <summary>Forma del cuerpo de error que devuelven los controllers: BadRequest(new { error = "..." }).</summary>
public class ApiErrorResponse
{
    public string? Error { get; set; }
}

public class ApiAuthService
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;
    private readonly ILogger<ApiAuthService> _logger;

    public ApiAuthService(HttpClient httpClient, LocalStorageService localStorage, ILogger<ApiAuthService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    // Las llamadas que emiten o consumen la cookie td_refresh necesitan credentials:include para
    // que el navegador la guarde y la envie en origen cruzado de desarrollo (:5147 -> :5100)
    // (auditoria 08-Oct H-04: el refresh token ya no viaja ni vive en el JS).
    private Task<HttpResponseMessage> PostWithCredentialsAsync<T>(string url, T payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(payload) };
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return _httpClient.SendAsync(request);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await PostWithCredentialsAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
    {
        var response = await PostWithCredentialsAsync("api/auth/register", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Register failed: {Error}", error);
            return new RegisterResult
            {
                EmailAlreadyRegistered = response.StatusCode == System.Net.HttpStatusCode.Conflict,
                ValidationError = response.StatusCode == System.Net.HttpStatusCode.BadRequest ? ReadMessage(error) : null
            };
        }
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return new RegisterResult { Data = result };
    }

    /// <summary>Registro de candidato, paso 1: pide el codigo al correo (todavia no se crea la cuenta).</summary>
    public async Task<EmailCodeResult> SendRegistrationCodeAsync(string email, string? firstName = null)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register/send-code", new RegistrationCodeRequestDto { Email = email, FirstName = firstName });
        if (response.IsSuccessStatusCode) return new EmailCodeResult(true, null);
        return new EmailCodeResult(false, response.StatusCode switch
        {
            System.Net.HttpStatusCode.Conflict => "email_exists",
            System.Net.HttpStatusCode.TooManyRequests => "too_soon",
            _ => ReadMessage(await response.Content.ReadAsStringAsync())
        });
    }

    public async Task<EmailVerificationStatusDto?> GetEmailVerificationStatusAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/auth/email-verification");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<EmailVerificationStatusDto>();
    }

    public async Task<EmailCodeResult> SendEmailVerificationCodeAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/auth/email-verification/send", null);
        if (response.IsSuccessStatusCode) return new EmailCodeResult(true, null);
        return new EmailCodeResult(false, response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
            ? "too_soon"
            : ReadMessage(await response.Content.ReadAsStringAsync()));
    }

    public async Task<EmailCodeResult> VerifyEmailCodeAsync(string code)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/auth/email-verification/verify", new VerifyEmailDto { Code = code });
        if (response.IsSuccessStatusCode) return new EmailCodeResult(true, null);
        return new EmailCodeResult(false, response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
            ? "too_many_attempts"
            : ReadMessage(await response.Content.ReadAsStringAsync()));
    }

    /// <summary>Lee { "message": "..." } del cuerpo de error; null si no tiene esa forma.</summary>
    private static string? ReadMessage(string body)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>Renueva la sesion con la cookie td_refresh (HttpOnly). Devuelve la respuesta si
    /// hubo sesion viva, null si no hay cookie o esta revocada/caducada.</summary>
    public async Task<AuthResponseDto?> TryRefreshSessionAsync()
    {
        var response = await PostWithCredentialsAsync("api/auth/refresh", new { });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    /// <summary>Revoca la sesion actual: el servidor lee la cookie td_refresh y la borra.</summary>
    public async Task<bool> RevokeSessionAsync()
    {
        await SetAuthHeaderAsync();
        var response = await PostWithCredentialsAsync("api/auth/revoke", new { });
        return response.IsSuccessStatusCode;
    }

    public async Task<CandidateDto?> GetCandidateProfileAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/candidates/me");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateDto>();
    }

    public async Task<CandidateDto?> UpdateWizardStepAsync(UpdateCandidateWizardDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/candidates/wizard", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateDto>();
    }

    public async Task<(List<TempVacancyDto> Items, int Total)> SearchVacanciesAsync(SearchVacancyDto search)
    {
        var query = $"api/vacancies/search?Page={search.Page}&PageSize={search.PageSize}";
        if (!string.IsNullOrEmpty(search.Query)) query += $"&Query={Uri.EscapeDataString(search.Query)}";
        if (!string.IsNullOrEmpty(search.Location)) query += $"&Location={Uri.EscapeDataString(search.Location)}";
        if (search.ContractType.HasValue) query += $"&ContractType={search.ContractType}";
        if (search.SalaryMin.HasValue) query += $"&SalaryMin={search.SalaryMin}";

        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return (new(), 0);

        var result = await response.Content.ReadFromJsonAsync<SearchResult>();
        return (result?.Items ?? new(), result?.Total ?? 0);
    }

    public async Task<List<TempVacancyDto>> GetMyVacanciesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/vacancies/my");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TempVacancyDto>>() ?? new();
    }

    public async Task<TempVacancyDto?> CreateTempVacancyAsync(CreateTempVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/vacancies/temp", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TempVacancyDto>();
    }

    public async Task<bool> DeleteTempVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/vacancies/temp/{id}");
        return response.IsSuccessStatusCode;
    }

    // === Phase 2: Security ===

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new { Token = token, NewPassword = newPassword });
        return response.IsSuccessStatusCode;
    }

    // --- Google (solo candidatos): el boton es un enlace a la API, que manda a Google y vuelve a /auth/google ---

    public async Task<bool> IsGoogleEnabledAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<GoogleEnabledResult>("api/auth/google/enabled");
            return result?.Enabled == true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>URL absoluta: en desarrollo la API va en otro puerto que el portal.</summary>
    public string GoogleStartUrl(string? returnUrl) =>
        new Uri(_httpClient.BaseAddress!, "api/auth/google/start").ToString()
        + (string.IsNullOrEmpty(returnUrl) ? "" : "?returnUrl=" + Uri.EscapeDataString(returnUrl));

    public async Task<AuthResponseDto?> GoogleExchangeAsync(string code)
    {
        var response = await PostWithCredentialsAsync("api/auth/google/exchange", new GoogleExchangeDto { Code = code });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<GoogleSignupInfoDto?> GetGoogleSignupInfoAsync(string ticket)
    {
        var response = await _httpClient.GetAsync($"api/auth/google/signup/{Uri.EscapeDataString(ticket)}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<GoogleSignupInfoDto>() : null;
    }

    public async Task<RegisterResult> GoogleSignupAsync(GoogleSignupDto dto)
    {
        var response = await PostWithCredentialsAsync("api/auth/google/signup", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Google signup failed: {Error}", error);
            return new RegisterResult
            {
                EmailAlreadyRegistered = response.StatusCode == System.Net.HttpStatusCode.Conflict,
                TicketExpired = response.StatusCode == System.Net.HttpStatusCode.NotFound,
                ValidationError = response.StatusCode == System.Net.HttpStatusCode.BadRequest ? ReadMessage(error) : null
            };
        }
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return new RegisterResult { Data = result };
    }

    private record GoogleEnabledResult(bool Enabled);

    // --- Noticias (publicas, sin login). Si la seccion esta apagada la API responde 404. ---

    public async Task<bool> IsNewsEnabledAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<GoogleEnabledResult>("api/news/enabled");
            return result?.Enabled == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<NewsPageDto?> GetNewsAsync(int? type, int page, int pageSize = 9)
    {
        var response = await _httpClient.GetAsync($"api/news?page={page}&pageSize={pageSize}" + (type != null ? $"&type={type}" : ""));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<NewsPageDto>() : null;
    }

    public async Task<NewsPostDetailDto?> GetNewsPostAsync(string slug)
    {
        var response = await _httpClient.GetAsync($"api/news/{Uri.EscapeDataString(slug)}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<NewsPostDetailDto>() : null;
    }

    /// <summary>URL absoluta de una foto publicada (en desarrollo la API va en otro puerto).</summary>
    public string? NewsImageUrl(string? fileName) => string.IsNullOrEmpty(fileName)
        ? null
        : new Uri(_httpClient.BaseAddress!, $"api/news/images/{Uri.EscapeDataString(fileName)}").ToString();

    public async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/verify-recaptcha", new { Response = recaptchaResponse });
        if (!response.IsSuccessStatusCode) return false;
        var result = await response.Content.ReadFromJsonAsync<RecaptchaResult>();
        return result?.Success ?? false;
    }

    // === Phase 2: Permanent Vacancies ===

    public async Task<(List<VacancyDto> Items, int Total)> SearchPermanentVacanciesAsync(SearchPermanentVacancyDto search)
    {
        var query = $"api/permanentvacancies/search?Page={search.Page}&PageSize={search.PageSize}";
        if (!string.IsNullOrEmpty(search.Query)) query += $"&Query={Uri.EscapeDataString(search.Query)}";
        if (!string.IsNullOrEmpty(search.Location)) query += $"&Location={Uri.EscapeDataString(search.Location)}";
        if (search.ContractType.HasValue) query += $"&ContractType={search.ContractType}";
        if (search.WorkMode.HasValue) query += $"&WorkMode={search.WorkMode}";
        if (!string.IsNullOrEmpty(search.Category)) query += $"&Category={Uri.EscapeDataString(search.Category)}";
        if (search.ExperienceLevel.HasValue) query += $"&ExperienceLevel={search.ExperienceLevel}";
        if (search.EnglishLevel.HasValue) query += $"&EnglishLevel={search.EnglishLevel}";
        if (search.SalaryMin.HasValue) query += $"&SalaryMin={search.SalaryMin}";
        if (search.SalaryMax.HasValue) query += $"&SalaryMax={search.SalaryMax}";
        if (!string.IsNullOrEmpty(search.SortBy)) query += $"&SortBy={Uri.EscapeDataString(search.SortBy)}";
        if (search.Latitude.HasValue) query += $"&Latitude={search.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (search.Longitude.HasValue) query += $"&Longitude={search.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (search.RadiusKm.HasValue) query += $"&RadiusKm={search.RadiusKm}";

        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return (new(), 0);

        var result = await response.Content.ReadFromJsonAsync<PermanentSearchResult>();
        return (result?.Items ?? new(), result?.Total ?? 0);
    }

    /// <summary>"Hacer Match": recalcula la compatibilidad del candidato contra todas las
    /// vacantes publicadas y devuelve las que tienen match (MatchPercentage lleno), ordenadas
    /// por porcentaje. Null si 401/403 o error (no es candidato logueado).</summary>
    public async Task<List<VacancyDto>?> GetMyMatchesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/permanentvacancies/my-matches", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
    }

    /// <summary>idOrCode: Guid interno o referencia publica TD-XXXXXXXX.</summary>
    public Task<VacancyDto?> GetPermanentVacancyAsync(Guid id) => GetPermanentVacancyAsync(id.ToString());

    /// <summary>idOrCode: Guid interno o referencia publica TD-XXXXXXXX.</summary>
    public async Task<VacancyDto?> GetPermanentVacancyAsync(string idOrCode)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/permanentvacancies/{Uri.EscapeDataString(idOrCode)}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VacancyDto>();
    }

    public async Task<List<VacancyDto>> GetMyCompanyVacanciesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/permanentvacancies/my-company");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<VacancyDto>>() ?? new();
    }

    public async Task<List<PlanDto>> GetPlansAsync(PlanAudience audience = PlanAudience.Company)
    {
        var response = await _httpClient.GetAsync($"api/plans?audience={audience}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<PlanDto>>() ?? new();
    }

    public async Task<bool> GetCandidatePlanFeatureEnabledAsync()
    {
        var response = await _httpClient.GetAsync("api/plans/candidate-enabled");
        if (!response.IsSuccessStatusCode) return false;
        var result = await response.Content.ReadFromJsonAsync<CandidatePlanFeatureResponse>();
        return result?.Enabled ?? false;
    }

    public async Task<bool> GetCompanyPlanFeatureEnabledAsync()
    {
        var response = await _httpClient.GetAsync("api/plans/company-enabled");
        if (!response.IsSuccessStatusCode) return true;
        var result = await response.Content.ReadFromJsonAsync<CandidatePlanFeatureResponse>();
        return result?.Enabled ?? true;
    }

    private class CandidatePlanFeatureResponse
    {
        public bool Enabled { get; set; }
    }

    public async Task<List<PublicCompanyDto>> GetPublicCompaniesAsync(int? limit = null)
    {
        var url = limit is > 0 ? $"api/companies?limit={limit.Value}" : "api/companies";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<PublicCompanyDto>>() ?? new();
    }

    public async Task<PublicCompanyDetailDto?> GetPublicCompanyAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/companies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PublicCompanyDetailDto>();
    }

    public async Task<List<VacancyDto>> GetCompanyVacanciesAsync(Guid companyId)
    {
        var response = await _httpClient.GetAsync($"api/permanentvacancies/company/{companyId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<VacancyDto>>() ?? new();
    }

    public async Task<MyCompanyProfileDto?> GetMyCompanyAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/companies/me");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MyCompanyProfileDto>();
    }

    public async Task<MyCompanyProfileDto?> UpdateMyCompanyAsync(UpdateMyCompanyProfileDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/companies/me", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MyCompanyProfileDto>();
    }

    public async Task<VerificationRequestResultDto?> GetMyVerificationRequestAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/verificationrequests/me");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VerificationRequestResultDto>();
    }

    public async Task<VerificationRequestResultDto?> SubmitVerificationRequestAsync(SubmitVerificationRequestDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/verificationrequests", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VerificationRequestResultDto>();
    }

    public async Task<List<JobTypeOptionDto>> GetJobTypesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/job-types");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobTypeOptionDto>>() ?? new();
    }

    public async Task<List<AdminSkillDto>> GetSkillsCatalogAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/skills");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminSkillDto>>() ?? new();
    }

    public async Task<List<ApplicationDto>> GetVacancyApplicationsAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/applications/vacancy/{vacancyId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDto>>() ?? new();
    }

    // === Embudo ciego: entregas de personal verificado ===

    public async Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid? vacancyId = null)
    {
        await SetAuthHeaderAsync();
        var url = vacancyId.HasValue ? $"api/deliveries/my?vacancyId={vacancyId}" : "api/deliveries/my";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<DeliveryDto>>() ?? new();
    }

    public async Task<VacancyApplicantSummaryDto?> GetVacancyApplicantSummaryAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/deliveries/vacancy-summary/{vacancyId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VacancyApplicantSummaryDto>();
    }

    /// <summary>rejectionReason (DeliveryRejectionReason) es obligatorio si status = RejectedByCompany.</summary>
    public async Task<bool> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback, int? rejectionReason = null)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/deliveries/{deliveryId}/respond",
            new RespondDeliveryDto { Status = status, Feedback = feedback, RejectionReason = rejectionReason });
        return response.IsSuccessStatusCode;
    }

    public async Task<VacancyDto?> CreateVacancyAsync(CreateVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/permanentvacancies", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VacancyDto>();
    }

    public async Task<PublishVacancyResult> PublishVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/permanentvacancies/{id}/publish", null);
        if (response.IsSuccessStatusCode) return new PublishVacancyResult(true, null);

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new PublishVacancyResult(false, error?.Error);
        }
        catch
        {
            return new PublishVacancyResult(false, null);
        }
    }

    public async Task<bool> CloseVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/permanentvacancies/{id}/close", null);
        return response.IsSuccessStatusCode;
    }

    /// <summary>Edita una vacante propia. Error = motivo del rechazo (p. ej. campo incluido en el contrato).</summary>
    public async Task<(VacancyDto? Result, string? Error)> UpdatePermanentVacancyAsync(Guid id, UpdateVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/permanentvacancies/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<VacancyDto>(), null);
        try
        {
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return (null, body != null && body.TryGetValue("error", out var e) ? e : null);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task<bool> DeleteVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/permanentvacancies/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConvertTempVacancyAsync(Guid tempVacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/permanentvacancies/convert-temp/{tempVacancyId}", null);
        return response.IsSuccessStatusCode;
    }

    // === Phase 2: Applications ===

    /// <summary>Error = mensaje del servidor cuando no se pudo postular (ya postulado, candidato Colocado, etc.).</summary>
    public async Task<(ApplicationDto? Result, string? Error)> ApplyAsync(CreateApplicationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/applications", dto);
        if (!response.IsSuccessStatusCode)
            return (null, (await response.Content.ReadAsStringAsync()).Trim('"'));
        return (await response.Content.ReadFromJsonAsync<ApplicationDto>(), null);
    }

    public async Task<List<ApplicationDto>> GetMyApplicationsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/applications/my");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDto>>() ?? new();
    }

    public async Task<CandidateProcessDto?> GetMyProcessAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/candidates/me/process");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateProcessDto>();
    }

    public async Task<List<ApplicationDto>> GetApplicationsByVacancyAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/applications/vacancy/{vacancyId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDto>>() ?? new();
    }

    /// <summary>Datos legales publicos de Trato Directo (politica de privacidad). No requiere sesion.</summary>
    public async Task<PublicLegalIdentityDto?> GetLegalIdentityAsync()
    {
        // Las paginas legales (/privacy, /cookies) deben mostrarse aunque el API falle: sin datos, muestran "—".
        try
        {
            var response = await _httpClient.GetAsync("api/legal/identity");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PublicLegalIdentityDto>();
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No se pudieron obtener los datos legales");
            return null;
        }
    }

    /// <summary>CV propio (null si no hay). Los CV ya no son archivos publicos: se bajan con sesion.</summary>
    public Task<(byte[] Content, string FileName)?> GetMyCvAsync() => DownloadCvAsync("api/profile/cv");

    /// <summary>CV de un candidato (el propio o uno entregado por TD a la empresa).</summary>
    public Task<(byte[] Content, string FileName)?> GetCandidateCvAsync(Guid candidateId) =>
        DownloadCvAsync($"api/profile/candidate/{candidateId}/cv");

    private async Task<(byte[] Content, string FileName)?> DownloadCvAsync(string url)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        var name = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "CV.pdf";
        return (await response.Content.ReadAsByteArrayAsync(), name);
    }

    public async Task<CandidateProfileDto?> GetCandidateProfileByIdAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/profile/candidate/{candidateId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
    }

    public async Task<ApplicationDto?> UpdateApplicationStatusAsync(Guid id, int status)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/applications/{id}/status", new UpdateApplicationStatusDto { Status = status });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApplicationDto>();
    }

    // === Phase 2: Profile (Experience, Education, Certification) ===

    public async Task<CandidateProfileDto?> GetProfileAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/profile");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
    }

    public async Task<CandidateProfileDto?> UpdateProfileAsync(UpdateCandidateProfileDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/profile", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
    }

    public async Task<CandidateExperienceDto?> AddExperienceAsync(CreateExperienceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/profile/experience", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateExperienceDto>();
    }

    public async Task<bool> DeleteExperienceAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/profile/experience/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<CandidateEducationDto?> AddEducationAsync(CreateEducationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/profile/education", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateEducationDto>();
    }

    public async Task<bool> DeleteEducationAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/profile/education/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<CandidateCertificationDto?> AddCertificationAsync(CreateCertificationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/profile/certification", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateCertificationDto>();
    }

    public async Task<bool> DeleteCertificationAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/profile/certification/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<UploadCvResponseDto?> UploadCvAsync(IBrowserFile file)
    {
        await SetAuthHeaderAsync();

        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _httpClient.PostAsync("api/profile/upload-cv", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UploadCvResponseDto>();
    }

    /// <summary>Foto de perfil propia como data URL (esta en almacenamiento privado del API, no es una URL publica).</summary>
    /// <summary>Si el admin tiene encendida la opcion de videos de presentacion.</summary>
    public async Task<bool> GetPresentationVideosEnabledAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/profile/video/enabled");
        if (!response.IsSuccessStatusCode) return false;
        var result = await response.Content.ReadFromJsonAsync<CandidatePlanFeatureResponse>();
        return result?.Enabled ?? false;
    }

    /// <summary>Borra el video de presentacion propio. La subida y la reproduccion las hace
    /// presentation-video.js directamente (ver GetApiAccessAsync).</summary>
    public async Task<bool> DeletePresentationVideoAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync("api/profile/video");
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetMyPhotoDataUrlAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/profile/photo");
        if (!response.IsSuccessStatusCode) return null;
        var type = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        return $"data:{type};base64,{Convert.ToBase64String(await response.Content.ReadAsByteArrayAsync())}";
    }

    /// <summary>Sube la foto de perfil. Devuelve null si todo fue bien, o el mensaje de error del API.</summary>
    public async Task<string?> UploadPhotoAsync(IBrowserFile file)
    {
        await SetAuthHeaderAsync();

        using var content = new MultipartFormDataContent();
        using var fileStream = file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _httpClient.PostAsync("api/profile/photo", content);
        return response.IsSuccessStatusCode ? null : (await response.Content.ReadAsStringAsync()).Trim('"');
    }

    public async Task<bool> DeletePhotoAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync("api/profile/photo");
        return response.IsSuccessStatusCode;
    }

    public async Task<CandidateProfileDto?> ApplyCvAsync(string cvUrl, CvParseResultDto parsedData)
    {
        await SetAuthHeaderAsync();

        var request = new ApplyCvRequestDto { CvUrl = cvUrl, ParsedData = parsedData };
        var response = await _httpClient.PostAsJsonAsync("api/profile/apply-cv", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateProfileDto>();
    }

    public async Task<List<AlertDto>> GetAlertsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/alerts");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AlertDto>>() ?? new();
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/messages/conversations");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ConversationDto>>() ?? new();
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/messages/{conversationId}/messages");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<MessageDto>>() ?? new();
    }

    public async Task<MessageDto?> SendMessageAsync(SendMessageDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/messages/send", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MessageDto>();
    }

    public async Task MarkConversationReadAsync(Guid conversationId)
    {
        await SetAuthHeaderAsync();
        await _httpClient.PutAsync($"api/messages/{conversationId}/read", null);
    }

    /// <summary>
    /// URL base del API y token actual, para las subidas y descargas grandes que hace JavaScript
    /// directamente (video de presentacion) sin pasar el archivo por la memoria de WebAssembly.
    /// </summary>
    public async Task<(string BaseUrl, string? Token)> GetApiAccessAsync() =>
        (_httpClient.BaseAddress?.ToString() ?? "/", await _localStorage.GetItemAsync("opentowork-token"));

    public async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("opentowork-token");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task PersistAuthAsync(AuthResponseDto auth)
    {
        await _localStorage.SetItemAsync("opentowork-token", auth.Token);
        // El refresh token vive en la cookie HttpOnly td_refresh; no se guarda en localStorage
        // (auditoria 08-Oct H-04). Se borra por si quedo uno viejo de antes del cambio.
        await _localStorage.RemoveItemAsync("opentowork-refresh-token");
        await _localStorage.SetItemAsync("opentowork-user-id", auth.User.Id.ToString());
        await _localStorage.SetItemAsync("opentowork-role", auth.User.PrimaryRole.ToString());
        await _localStorage.SetItemAsync("opentowork-theme", auth.User.Theme ?? "navy");
        await _localStorage.SetItemAsync("opentowork-lang", auth.User.Language ?? "es");
    }

    public async Task ClearAuthAsync()
    {
        await _localStorage.RemoveItemAsync("opentowork-token");
        await _localStorage.RemoveItemAsync("opentowork-refresh-token");
        await _localStorage.RemoveItemAsync("opentowork-user-id");
        await _localStorage.RemoveItemAsync("opentowork-role");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // --- Fase 3 (Scoring/Verificaciones/Referencias/Retos) - sub-fase 3.8 ---

    public async Task<CandidateScoreDto?> GetScoreAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/candidates/{candidateId}/score");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateScoreDto>();
    }

    public async Task<CandidateScoreDto?> RecalculateScoreAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/candidates/{candidateId}/score/recalculate", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateScoreDto>();
    }

    public async Task<VerificationStatusDto?> GetVerificationStatusAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/candidates/{candidateId}/verification-status");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VerificationStatusDto>();
    }

    public async Task<List<VerificationResultDto>> GetVerificationsAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/candidates/{candidateId}/verifications");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<VerificationResultDto>>() ?? new();
    }

    public async Task<List<VerificationResultDto>> RunVerificationsAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/candidates/{candidateId}/verifications/run", null);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<VerificationResultDto>>() ?? new();
    }

    public async Task<CandidateReferencesListDto?> GetReferencesAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/candidates/{candidateId}/references");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateReferencesListDto>();
    }

    public async Task<CandidateReferenceDto?> AddReferenceAsync(Guid candidateId, CreateReferenceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/candidates/{candidateId}/references", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateReferenceDto>();
    }

    public async Task<ReferenceRequestLinkDto?> SendReferenceRequestAsync(Guid referenceId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/references/{referenceId}/send", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ReferenceRequestLinkDto>();
    }

    public async Task<List<SkillTestPublicDto>> GetAvailableSkillTestsAsync(string? category = null)
    {
        await SetAuthHeaderAsync();
        var url = string.IsNullOrEmpty(category) ? "api/skill-tests/available" : $"api/skill-tests/available?category={Uri.EscapeDataString(category)}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<SkillTestPublicDto>>() ?? new();
    }

    public async Task<(TestAttemptDto? Attempt, string? Error)> StartSkillTestAsync(Guid testId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/skill-tests/{testId}/start", null);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return (null, body);
        }
        return (await response.Content.ReadFromJsonAsync<TestAttemptDto>(), null);
    }

    public async Task<TestResultDto?> SubmitSkillTestAsync(Guid resultId, SubmitTestAnswersDto dto, int antiCheatFlags)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/skill-tests/results/{resultId}/submit?antiCheatFlags={antiCheatFlags}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TestResultDto>();
    }

    public async Task<List<TestResultDto>> GetMySkillTestResultsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/skill-tests/results");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TestResultDto>>() ?? new();
    }

    public async Task<List<JobMatchDto>> GetVacancyMatchesAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/permanentvacancies/{vacancyId}/matches");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobMatchDto>>() ?? new();
    }

    public async Task<ScorecardDto?> GetVacancyScorecardAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/permanentvacancies/{vacancyId}/scorecard");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ScorecardDto>();
    }

    public async Task<bool> UpdateVacancyScorecardAsync(Guid vacancyId, UpdateScorecardDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/permanentvacancies/{vacancyId}/scorecard", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetTokenAsync() => await _localStorage.GetItemAsync("opentowork-token");
    public async Task<string?> GetUserIdAsync() => await _localStorage.GetItemAsync("opentowork-user-id");
    public async Task<string?> GetUserRoleAsync() => await _localStorage.GetItemAsync("opentowork-role");

    private class SearchResult
    {
        public List<TempVacancyDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    private class PermanentSearchResult
    {
        public List<VacancyDto> Items { get; set; } = new();
        public int Total { get; set; }
    }

    // ---------------- Retos de hosteleria (api/challenges, api/challenge-attempts) ----------------
    // Devuelven (valor, codigo de error de la API: "notFound", "cooldown", "locked"...).

    public Task<(List<OpenToWork.Shared.Challenges.ChallengeJobTypeDto>?, string?)> GetChallengeJobTypesAsync() =>
        ChallengeCallAsync<List<OpenToWork.Shared.Challenges.ChallengeJobTypeDto>>(HttpMethod.Get, "api/challenges/job-types");

    public Task<(List<OpenToWork.Shared.Challenges.ChallengeCardDto>?, string?)> GetChallengeCatalogAsync(Guid jobTypeId) =>
        ChallengeCallAsync<List<OpenToWork.Shared.Challenges.ChallengeCardDto>>(HttpMethod.Get, $"api/challenges/job-types/{jobTypeId}");

    public Task<(OpenToWork.Shared.Challenges.ChallengeIntroDto?, string?)> GetChallengeIntroAsync(Guid id) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.ChallengeIntroDto>(HttpMethod.Get, $"api/challenges/{id}");

    public Task<(OpenToWork.Shared.Challenges.AttemptViewDto?, string?)> StartChallengeAttemptAsync(Guid id, OpenToWork.Shared.Challenges.StartAttemptDto dto) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.AttemptViewDto>(HttpMethod.Post, $"api/challenges/{id}/attempts", dto);

    public Task<(OpenToWork.Shared.Challenges.AttemptViewDto?, string?)> GetChallengeAttemptAsync(Guid attemptId) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.AttemptViewDto>(HttpMethod.Get, $"api/challenge-attempts/{attemptId}");

    public Task<(OpenToWork.Shared.Challenges.SaveAnswerResultDto?, string?)> SaveChallengeAnswerAsync(Guid attemptId, string activityKey, OpenToWork.Shared.Challenges.ChallengeResponse response) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.SaveAnswerResultDto>(HttpMethod.Put, $"api/challenge-attempts/{attemptId}/answers/{Uri.EscapeDataString(activityKey)}", response);

    public Task<(OpenToWork.Shared.Challenges.AttemptResultDto?, string?)> SubmitChallengeAttemptAsync(Guid attemptId) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.AttemptResultDto>(HttpMethod.Post, $"api/challenge-attempts/{attemptId}/submit");

    public Task<(OpenToWork.Shared.Challenges.AttemptResultDto?, string?)> GetChallengeResultAsync(Guid attemptId) =>
        ChallengeCallAsync<OpenToWork.Shared.Challenges.AttemptResultDto>(HttpMethod.Get, $"api/challenge-attempts/{attemptId}/result");

    public Task<(List<OpenToWork.Shared.Challenges.AttemptResultDto>?, string?)> GetChallengeHistoryAsync() =>
        ChallengeCallAsync<List<OpenToWork.Shared.Challenges.AttemptResultDto>>(HttpMethod.Get, "api/challenge-attempts/history");

    public Task<(List<OpenToWork.Shared.Challenges.CompanyChallengeResultDto>?, string?)> GetCandidateChallengeResultsForCompanyAsync(Guid candidateId) =>
        ChallengeCallAsync<List<OpenToWork.Shared.Challenges.CompanyChallengeResultDto>>(HttpMethod.Get, $"api/company/candidates/{candidateId}/challenge-results");

    private async Task<(T?, string?)> ChallengeCallAsync<T>(HttpMethod method, string url, object? body = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            using var request = new HttpRequestMessage(method, url);
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<T>(), null);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return (default, "unauthorized"); // cuerpo vacio
            try { return (default, (await response.Content.ReadFromJsonAsync<ChallengeError>())?.Error ?? "generic"); }
            catch (System.Text.Json.JsonException) { return (default, response.StatusCode == System.Net.HttpStatusCode.Forbidden ? "forbidden" : "generic"); }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Retos: fallo llamando a {Url}", url);
            return (default, "generic");
        }
    }

    private class ChallengeError
    {
        public string? Error { get; set; }
    }

    private class RecaptchaResult
    {
        public bool Success { get; set; }
    }
}
