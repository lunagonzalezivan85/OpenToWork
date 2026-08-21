using System.Net.Http.Headers;
using System.Net.Http.Json;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.AdminWEB.Services;

public class AdminAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;

    public AdminAuthApiService(HttpClient httpClient, LocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return result;
    }

    public async Task<List<AuditLogDto>> GetAuditLogAsync(int page = 1, int pageSize = 20)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/audit-log?page={page}&pageSize={pageSize}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AuditLogDto>>() ?? new();
    }

    public async Task<DashboardMetricsDto?> GetMetricsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/dashboard/metrics");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DashboardMetricsDto>();
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(int page = 1, int pageSize = 1000, int? role = null)
    {
        await SetAuthHeaderAsync();
        var url = role.HasValue
            ? $"api/admin/users?page={page}&pageSize={pageSize}&role={role.Value}"
            : $"api/admin/users?page={page}&pageSize={pageSize}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminUserDto>>() ?? new();
    }

    public async Task<bool> ActivateUserAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/users/{id}/activate", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/users/{id}/deactivate", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<AdminUserProfileDto?> GetUserProfileAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/users/{id}/profile");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminUserProfileDto>();
    }

    public async Task<List<AdminVacancyDto>> GetVacanciesAsync(int page = 1, int pageSize = 20, int? status = null)
    {
        await SetAuthHeaderAsync();
        var query = $"api/admin/vacancies?page={page}&pageSize={pageSize}";
        if (status.HasValue) query += $"&status={status}";
        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminVacancyDto>>() ?? new();
    }

    public async Task<List<AdminApplicationDto>> GetApplicationsAsync(int page = 1, int pageSize = 20, int? status = null)
    {
        await SetAuthHeaderAsync();
        var query = $"api/admin/applications?page={page}&pageSize={pageSize}";
        if (status.HasValue) query += $"&status={status}";
        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminApplicationDto>>() ?? new();
    }

    public async Task<bool> ModerateVacancyAsync(Guid id, int status)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/vacancies/{id}/moderate", new ModerateVacancyDto { Status = status });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AdminSkillDto>> GetSkillsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/skills");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminSkillDto>>() ?? new();
    }

    public async Task<AdminSkillDto?> CreateSkillAsync(CreateSkillDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/skills", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminSkillDto>();
    }

    public async Task<bool> DeleteSkillAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/skills/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<byte[]?> ExportUsersCsvAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/export/users");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]?> ExportVacanciesCsvAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/export/vacancies");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<CandidateConsoleResultDto?> GetCandidatesAsync(
        int page = 1, int pageSize = 20, string? search = null,
        bool? wizardCompleted = null, bool? hasLinkedIn = null,
        bool? hasPortfolio = null, bool? hasCV = null, bool? isActive = null,
        Guid? skillId = null, string? sortBy = null, bool sortDesc = true,
        string? recruitmentStatus = null)
    {
        await SetAuthHeaderAsync();
        var query = $"api/admin/candidates?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (wizardCompleted.HasValue) query += $"&wizardCompleted={wizardCompleted.Value}";
        if (hasLinkedIn.HasValue) query += $"&hasLinkedIn={hasLinkedIn.Value}";
        if (hasPortfolio.HasValue) query += $"&hasPortfolio={hasPortfolio.Value}";
        if (hasCV.HasValue) query += $"&hasCV={hasCV.Value}";
        if (isActive.HasValue) query += $"&isActive={isActive.Value}";
        if (skillId.HasValue) query += $"&skillId={skillId.Value}";
        if (!string.IsNullOrEmpty(sortBy)) query += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        query += $"&sortDesc={sortDesc.ToString().ToLower()}";
        if (!string.IsNullOrEmpty(recruitmentStatus)) query += $"&recruitmentStatus={recruitmentStatus}";

        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateConsoleResultDto>();
    }

    public async Task<bool> BulkActivateCandidatesAsync(List<Guid> ids)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/candidates/bulk-activate", new { Ids = ids });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> BulkDeactivateCandidatesAsync(List<Guid> ids)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/candidates/bulk-deactivate", new { Ids = ids });
        return response.IsSuccessStatusCode;
    }

    public async Task<byte[]?> ExportCandidatesCsvAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/candidates/export");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<RecruitmentPipelineResultDto?> GetRecruitmentPipelineAsync(
        int page = 1, int pageSize = 20, int? stage = null, Guid? assignedTo = null, string? search = null)
    {
        await SetAuthHeaderAsync();
        var query = $"api/admin/recruitment?page={page}&pageSize={pageSize}";
        if (stage.HasValue) query += $"&stage={stage.Value}";
        if (assignedTo.HasValue) query += $"&assignedTo={assignedTo.Value}";
        if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecruitmentPipelineResultDto>();
    }

    public async Task<RecruitmentDetailDto?> GetRecruitmentDetailAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/recruitment/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecruitmentDetailDto>();
    }

    public async Task<RecruitmentPipelineDto?> AssignCandidateAsync(AssignCandidateDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/recruitment/assign", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecruitmentPipelineDto>();
    }

    public async Task<bool> MoveStageAsync(Guid recruitmentId, int toStage, string? notes = null)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/move-stage", new MoveStageDto { ToStage = toStage, Notes = notes });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleInvestigationStepAsync(Guid recruitmentId, ToggleInvestigationStepDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/investigation", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<InvestigationChecklistDto?> AddCustomValidationAsync(Guid recruitmentId, string label)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment/{recruitmentId}/investigation/custom", new AddCustomValidationDto { Label = label });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvestigationChecklistDto>();
    }

    public async Task<bool> DeleteCustomValidationAsync(Guid checklistId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/recruitment/investigation/{checklistId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DismissCandidateAsync(Guid recruitmentId, int reason, string? notes = null)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment/{recruitmentId}/dismiss", new DismissCandidateDto { Reason = reason, Notes = notes });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnassignCandidateAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/recruitment/{recruitmentId}/unassign", null);
        return response.IsSuccessStatusCode;
    }

    public async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync("otwadmin-token");
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task PersistAuthAsync(AuthResponseDto auth)
    {
        await _localStorage.SetItemAsync("otwadmin-token", auth.Token);
        await _localStorage.SetItemAsync("otwadmin-refresh-token", auth.RefreshToken);
        await _localStorage.SetItemAsync("otwadmin-user-id", auth.User.Id.ToString());
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("otwadmin-token");
        await _localStorage.RemoveItemAsync("otwadmin-refresh-token");
        await _localStorage.RemoveItemAsync("otwadmin-user-id");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
