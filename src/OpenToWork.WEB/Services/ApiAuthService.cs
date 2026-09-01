using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using OpenToWork.Shared.DTOs;
using OpenToWork.SharedUI.Services;

namespace OpenToWork.WEB.Services;

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

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Register failed: {Error}", error);
            return null;
        }
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenDto { RefreshToken = refreshToken });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/revoke", new RefreshTokenDto { RefreshToken = refreshToken });
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

    public async Task<AuthResponseDto?> GoogleLoginAsync(string googleToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/google", new { Token = googleToken });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

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

        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return (new(), 0);

        var result = await response.Content.ReadFromJsonAsync<PermanentSearchResult>();
        return (result?.Items ?? new(), result?.Total ?? 0);
    }

    public async Task<VacancyDto?> GetPermanentVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/permanentvacancies/{id}");
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

    public async Task<VacancyDto?> CreateVacancyAsync(CreateVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/permanentvacancies", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VacancyDto>();
    }

    public async Task<bool> PublishVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/permanentvacancies/{id}/publish", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CloseVacancyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/permanentvacancies/{id}/close", null);
        return response.IsSuccessStatusCode;
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

    public async Task<ApplicationDto?> ApplyAsync(CreateApplicationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/applications", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApplicationDto>();
    }

    public async Task<List<ApplicationDto>> GetMyApplicationsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/applications/my");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDto>>() ?? new();
    }

    public async Task<List<ApplicationDto>> GetApplicationsByVacancyAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/applications/vacancy/{vacancyId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ApplicationDto>>() ?? new();
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

    public async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("opentowork-token");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task PersistAuthAsync(AuthResponseDto auth)
    {
        await _localStorage.SetItemAsync("opentowork-token", auth.Token);
        await _localStorage.SetItemAsync("opentowork-refresh-token", auth.RefreshToken);
        await _localStorage.SetItemAsync("opentowork-user-id", auth.User.Id.ToString());
        await _localStorage.SetItemAsync("opentowork-theme", auth.User.Theme ?? "navy");
        await _localStorage.SetItemAsync("opentowork-lang", auth.User.Language ?? "es");
    }

    public async Task ClearAuthAsync()
    {
        await _localStorage.RemoveItemAsync("opentowork-token");
        await _localStorage.RemoveItemAsync("opentowork-refresh-token");
        await _localStorage.RemoveItemAsync("opentowork-user-id");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> GetTokenAsync() => await _localStorage.GetItemAsync("opentowork-token");
    public async Task<string?> GetRefreshTokenAsync() => await _localStorage.GetItemAsync("opentowork-refresh-token");
    public async Task<string?> GetUserIdAsync() => await _localStorage.GetItemAsync("opentowork-user-id");

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

    private class RecaptchaResult
    {
        public bool Success { get; set; }
    }
}
