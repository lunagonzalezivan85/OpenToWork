using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;
using OpenToWork.SharedUI.Services;

namespace OpenToWork.AdminWEB.Services;

public class AdminLoginResult
{
    public AuthResponseDto? Data { get; set; }
    public bool PasswordExpired { get; set; }
}

/// <summary>Resultado de crear/guardar un contrato: si falla, trae el mensaje real del servidor
/// (precio de lista faltante, codigo promocional invalido, etc.) en vez de un error generico.</summary>
public record ContractSaveResult(AdminVacancyContractDto? Contract, string? Error);

/// <summary>Forma del cuerpo de error que devuelven los controllers admin: BadRequest(new { error = "..." }).</summary>
public class ApiErrorResponse
{
    public string? Error { get; set; }
}

public class AdminAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;

    public AdminAuthApiService(HttpClient httpClient, LocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AdminLoginResult> LoginAsync(LoginDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/auth/login", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new AdminLoginResult { PasswordExpired = error.Contains("Password expired") };
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (result != null) await PersistAuthAsync(result);
        return new AdminLoginResult { Data = result };
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

    public async Task<BusinessMetricsDto?> GetBusinessMetricsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/dashboard/business-metrics");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<BusinessMetricsDto>();
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

    public async Task<(AdminUserDto? User, string? Error)> CreateUserAsync(CreateUserDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/users", dto);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<AdminUserDto>(), null);
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return (null, error?.Error ?? "Error al crear usuario");
        }
        catch
        {
            return (null, "Error al crear usuario");
        }
    }

    public async Task<bool> ChangeUserRoleAsync(Guid id, int role)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{id}/role", new ChangeRoleDto { Role = role });
        return response.IsSuccessStatusCode;
    }

    public async Task<AdminUserProfileDto?> GetUserProfileAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/users/{id}/profile");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminUserProfileDto>();
    }

    public async Task<AdminVacancyResultDto> GetVacanciesAsync(int page = 1, int pageSize = 20, int? status = null, Guid? companyId = null)
    {
        await SetAuthHeaderAsync();
        var query = $"api/admin/vacancies?page={page}&pageSize={pageSize}";
        if (status.HasValue) query += $"&status={status}";
        if (companyId.HasValue) query += $"&companyId={companyId}";
        var response = await _httpClient.GetAsync(query);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<AdminVacancyResultDto>() ?? new();
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

    public async Task<VacancyDto?> GetVacancyDetailAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/vacancies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VacancyDto>();
    }

    public async Task<List<AdminApplicationDto>> GetVacancyApplicantsAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/vacancies/{vacancyId}/applicants");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminApplicationDto>>() ?? new();
    }

    public async Task<List<JobMatchDto>> GetVacancyNonApplicantMatchesAsync(Guid vacancyId, int? minPercentage = null)
    {
        await SetAuthHeaderAsync();
        var url = $"api/admin/vacancies/{vacancyId}/non-applicant-matches";
        if (minPercentage.HasValue && minPercentage.Value > 0) url += $"?minPercentage={minPercentage.Value}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobMatchDto>>() ?? new();
    }

    public async Task<bool> ApplyCandidateToVacancyAsync(Guid vacancyId, Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/vacancies/{vacancyId}/applications", new AdminCreateApplicationDto { CandidateId = candidateId });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SystemConfigDto>?> GetSystemConfigAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/system-config");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<SystemConfigDto>>();
    }

    public async Task<bool> UpdateSystemConfigAsync(UpdateSystemConfigDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/admin/system-config", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> GetCandidatePriorityPlanEnabledAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/system-config/candidate-priority-plan");
        if (!response.IsSuccessStatusCode) return false;
        var result = await response.Content.ReadFromJsonAsync<FeatureFlagResponse>();
        return result?.Enabled ?? false;
    }

    public async Task<bool> SetCandidatePriorityPlanEnabledAsync(bool enabled)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/admin/system-config/candidate-priority-plan", new { Enabled = enabled });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> GetCompanyPlansEnabledAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/system-config/company-plans");
        if (!response.IsSuccessStatusCode) return true;
        var result = await response.Content.ReadFromJsonAsync<FeatureFlagResponse>();
        return result?.Enabled ?? true;
    }

    public async Task<bool> SetCompanyPlansEnabledAsync(bool enabled)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/admin/system-config/company-plans", new { Enabled = enabled });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetCandidatePlanTierAsync(Guid scUserId, CandidatePlanTier tier)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{scUserId}/plan-tier", new { Tier = tier });
        return response.IsSuccessStatusCode;
    }

    private class FeatureFlagResponse
    {
        public bool Enabled { get; set; }
    }

    public async Task<CompanyIdentityDto?> GetCompanyIdentityAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/contracts/company-identity");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyIdentityDto>();
    }

    public async Task<AdminVacancyContractDto?> GetContractAsync(Guid contractId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/{contractId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminVacancyContractDto>();
    }

    public async Task<AdminVacancyContractDto?> GetContractByCompanyAsync(Guid companyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/by-company/{companyId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminVacancyContractDto>();
    }

    public async Task<ContractSaveResult> CreateContractAsync(Guid companyId, AdminSaveVacancyContractDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/contracts/by-company/{companyId}", dto);
        return await ReadContractSaveResultAsync(response);
    }

    public async Task<ContractSaveResult> SaveContractAsync(Guid contractId, AdminSaveVacancyContractDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/contracts/{contractId}", dto);
        return await ReadContractSaveResultAsync(response);
    }

    private static async Task<ContractSaveResult> ReadContractSaveResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return new ContractSaveResult(await response.Content.ReadFromJsonAsync<AdminVacancyContractDto>(), null);

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return new ContractSaveResult(null, error?.Error);
        }
        catch
        {
            return new ContractSaveResult(null, null);
        }
    }

    public async Task<PromoValidationResultDto?> ValidatePromoCodeAsync(string code, Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/contracts/validate-promo", new ValidatePromoCodeDto { Code = code, VacancyId = vacancyId });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PromoValidationResultDto>();
    }

    // ===== Pricing: niveles/tipos de puesto, lista de precios =====

    public async Task<List<JobLevelDto>> GetJobLevelsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/pricing/job-levels");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobLevelDto>>() ?? new();
    }

    public async Task<(JobLevelDto? Result, string? Error)> CreateJobLevelAsync(SaveJobLevelDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/pricing/job-levels", dto);
        return await ReadResultAsync<JobLevelDto>(response);
    }

    public async Task<(JobLevelDto? Result, string? Error)> UpdateJobLevelAsync(Guid id, SaveJobLevelDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/pricing/job-levels/{id}", dto);
        return await ReadResultAsync<JobLevelDto>(response);
    }

    public async Task<bool> DeleteJobLevelAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/pricing/job-levels/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<JobTypeDto>> GetJobTypesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/pricing/job-types");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobTypeDto>>() ?? new();
    }

    public async Task<(JobTypeDto? Result, string? Error)> CreateJobTypeAsync(SaveJobTypeDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/pricing/job-types", dto);
        return await ReadResultAsync<JobTypeDto>(response);
    }

    public async Task<(JobTypeDto? Result, string? Error)> UpdateJobTypeAsync(Guid id, SaveJobTypeDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/pricing/job-types/{id}", dto);
        return await ReadResultAsync<JobTypeDto>(response);
    }

    public async Task<bool> DeleteJobTypeAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/pricing/job-types/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<JobTypePriceDto>> GetPriceHistoryAsync(Guid jobTypeId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/pricing/job-types/{jobTypeId}/price-history");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobTypePriceDto>>() ?? new();
    }

    public async Task<(JobTypePriceDto? Result, string? Error)> SetJobTypePriceAsync(Guid jobTypeId, SetJobTypePriceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/pricing/job-types/{jobTypeId}/price", dto);
        return await ReadResultAsync<JobTypePriceDto>(response);
    }

    public async Task<List<AdminSkillDto>> GetJobTypeSkillsAsync(Guid jobTypeId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/pricing/job-types/{jobTypeId}/skills");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminSkillDto>>() ?? new();
    }

    public async Task<(List<AdminSkillDto>? Result, string? Error)> SetJobTypeSkillsAsync(Guid jobTypeId, List<Guid> skillIds)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/pricing/job-types/{jobTypeId}/skills", new SetJobTypeSkillsDto { SkillIds = skillIds });
        return await ReadResultAsync<List<AdminSkillDto>>(response);
    }

    // ===== Codigos promocionales =====

    public async Task<List<PromoCodeDto>> GetPromoCodesAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/promo-codes");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<PromoCodeDto>>() ?? new();
    }

    public async Task<(PromoCodeDto? Result, string? Error)> CreatePromoCodeAsync(SavePromoCodeDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/promo-codes", dto);
        return await ReadResultAsync<PromoCodeDto>(response);
    }

    public async Task<(PromoCodeDto? Result, string? Error)> UpdatePromoCodeAsync(Guid id, SavePromoCodeDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/promo-codes/{id}", dto);
        return await ReadResultAsync<PromoCodeDto>(response);
    }

    public async Task<bool> DeletePromoCodeAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/promo-codes/{id}");
        return response.IsSuccessStatusCode;
    }

    // ===== Planes (catalogo PT_Plans, gestion SuperAdmin) =====

    public async Task<List<PlanDto>> GetAllPlansAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/company-crm/plans/all");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<PlanDto>>() ?? new();
    }

    public async Task<(PlanDto? Result, string? Error)> CreatePlanAsync(SavePlanDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/company-crm/plans", dto);
        return await ReadResultAsync<PlanDto>(response);
    }

    public async Task<(PlanDto? Result, string? Error)> UpdatePlanAsync(Guid id, SavePlanDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/plans/{id}", dto);
        return await ReadResultAsync<PlanDto>(response);
    }

    public async Task<bool> DeletePlanAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/company-crm/plans/{id}");
        return response.IsSuccessStatusCode;
    }

    // ===== SMTP / Notificaciones por Email =====

    public async Task<SmtpSettingsDto?> GetSmtpSettingsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/email/settings");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SmtpSettingsDto>();
    }

    public async Task<(SmtpSettingsDto? Result, string? Error)> UpdateSmtpSettingsAsync(SmtpSettingsDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/admin/email/settings", dto);
        return await ReadResultAsync<SmtpSettingsDto>(response);
    }

    public async Task<(bool Success, string? Error)> SendTestEmailAsync(string toEmail)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/email/test", new SendTestEmailDto { ToEmail = toEmail });
        if (response.IsSuccessStatusCode) return (true, null);
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return (false, error?.Error ?? "No se pudo enviar el correo de prueba.");
        }
        catch
        {
            return (false, "No se pudo enviar el correo de prueba.");
        }
    }

    private static async Task<(T? Result, string? Error)> ReadResultAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<T>(), null);
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return (default, error?.Error);
        }
        catch
        {
            return (default, null);
        }
    }

    public async Task<(AdminVacancyContractDto? Result, string? Error)> ReopenContractAsync(Guid contractId, string reason)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/contracts/{contractId}/reopen", new ReopenContractDto { Reason = reason });
        return await ReadResultAsync<AdminVacancyContractDto>(response);
    }

    public async Task<List<ContractRevisionDto>> GetContractRevisionsAsync(Guid contractId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/{contractId}/revisions");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ContractRevisionDto>>() ?? new();
    }

    public async Task<bool> SendContractAsync(Guid contractId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/contracts/{contractId}/send", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DecideContractAsync(Guid contractId, bool accepted, string? reason = null)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/contracts/{contractId}/decision", new AdminContractDecisionDto { Accepted = accepted, Reason = reason });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ContractPaymentDto>> GetContractPaymentsAsync(Guid contractId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/{contractId}/payments");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ContractPaymentDto>>() ?? new();
    }

    public async Task<bool> MarkTranchePaidAsync(Guid trancheId, string? notes)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/contracts/payments/{trancheId}/mark-paid", new MarkTranchePaidDto { Notes = notes });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<WarrantyReplacementDto>> GetWarrantyReplacementsByContractAsync(Guid contractId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/{contractId}/warranty-replacements");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<WarrantyReplacementDto>>() ?? new();
    }

    public async Task<WarrantyReplacementCandidatesDto> GetWarrantyReplacementCandidatesAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/contracts/warranty-replacements/vacancy/{vacancyId}/candidates");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<WarrantyReplacementCandidatesDto>() ?? new();
    }

    public async Task<(WarrantyReplacementDto? Result, string? Error)> LinkWarrantyReplacementAsync(Guid replacementId, LinkWarrantyReplacementDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/contracts/warranty-replacements/{replacementId}/link", dto);
        return await ReadResultAsync<WarrantyReplacementDto>(response);
    }

    public async Task<(WarrantyReplacementDto? Result, string? Error)> CancelWarrantyReplacementAsync(Guid replacementId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/contracts/warranty-replacements/{replacementId}/cancel", null);
        return await ReadResultAsync<WarrantyReplacementDto>(response);
    }

    public async Task<AdminVacancyDto?> CreateVacancyAsync(AdminCreateVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/vacancies", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminVacancyDto>();
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

    public async Task<RecruitmentDetailDto?> GetRecruitmentByUserAsync(Guid userId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/recruitment/by-user/{userId}");
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

    public async Task<MoveStageResultDto> MoveStageAsync(Guid recruitmentId, int toStage, string? notes = null)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/move-stage", new MoveStageDto { ToStage = toStage, Notes = notes });
        if (response.IsSuccessStatusCode)
            return new MoveStageResultDto { Success = true };

        var errorBody = await response.Content.ReadAsStringAsync();
        return new MoveStageResultDto { Success = false, Error = errorBody };
    }

    public async Task<bool> ToggleInvestigationStepAsync(Guid recruitmentId, ToggleInvestigationStepDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/investigation", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> StartInvestigationStepAsync(Guid recruitmentId, int step)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/recruitment/{recruitmentId}/investigation/{step}/start", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<ReferenceCheckDto?> AddReferenceAsync(Guid checklistId, AddReferenceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment/investigation/{checklistId}/references", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ReferenceCheckDto>();
    }

    public async Task<bool> UpdateReferenceStatusAsync(Guid referenceId, UpdateReferenceStatusDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/references/{referenceId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteReferenceAsync(Guid referenceId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/recruitment/references/{referenceId}");
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

    public async Task<bool> RestoreCandidateAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/recruitment/{recruitmentId}/restore", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnassignCandidateAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/recruitment/{recruitmentId}/unassign", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCandidatePhoneAsync(Guid recruitmentId, string? phone)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/candidate-phone", new UpdateCandidatePhoneDto { Phone = phone });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateChecklistNotesAsync(Guid checklistId, string? notes)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/investigation/{checklistId}/notes", new UpdateChecklistNotesDto { Notes = notes });
        return response.IsSuccessStatusCode;
    }

    public async Task<TechnicalEvaluationDto?> AddTechnicalEvaluationAsync(Guid recruitmentId, AddTechnicalEvaluationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment/{recruitmentId}/evaluations", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TechnicalEvaluationDto>();
    }

    public async Task<bool> UpdateTechnicalEvaluationAsync(Guid evaluationId, UpdateTechnicalEvaluationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/evaluations/{evaluationId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTechnicalEvaluationAsync(Guid evaluationId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/recruitment/evaluations/{evaluationId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TechnicalEvaluationDto?> GetCulturalInterviewAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/recruitment/{recruitmentId}/cultural-interview");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TechnicalEvaluationDto>();
    }

    public async Task<AdminRegisterCandidateResultDto?> RegisterCandidateFromCvAsync(MultipartFormDataContent content)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/admin/candidates/register-cv", content);
        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            var msg = !string.IsNullOrWhiteSpace(errorJson) ? errorJson : $"Error HTTP {(int)response.StatusCode}";
            return new AdminRegisterCandidateResultDto { Success = false, Error = msg };
        }
        return await response.Content.ReadFromJsonAsync<AdminRegisterCandidateResultDto>();
    }

    public async Task<AdminRegisterCandidateResultDto?> RegisterCandidateManualAsync(AdminRegisterCandidateManualDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/candidates/register-manual", dto);
        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            var msg = !string.IsNullOrWhiteSpace(errorJson) ? errorJson : $"Error HTTP {(int)response.StatusCode}";
            return new AdminRegisterCandidateResultDto { Success = false, Error = msg };
        }
        return await response.Content.ReadFromJsonAsync<AdminRegisterCandidateResultDto>();
    }

    public async Task<LinkedinSearchResponseDto?> SearchLinkedinAsync(LinkedinSearchRequestDto request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/candidates/search-linkedin", request);
        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            var msg = !string.IsNullOrWhiteSpace(errorJson) ? errorJson : $"Error HTTP {(int)response.StatusCode}";
            return new LinkedinSearchResponseDto { Success = false, Error = msg };
        }
        return await response.Content.ReadFromJsonAsync<LinkedinSearchResponseDto>();
    }

    public async Task<CandidateRecruitmentPreferencesDto?> GetRecruitmentPreferencesAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/recruitment/{recruitmentId}/preferences");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateRecruitmentPreferencesDto>();
    }

    public async Task<CandidateRecruitmentPreferencesDto?> SaveRecruitmentPreferencesAsync(
        Guid recruitmentId, UpdateRecruitmentPreferencesDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/preferences", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateRecruitmentPreferencesDto>();
    }

    public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<DocumentTypeDto>>("api/admin/recruitment/document-types") ?? new();
    }

    public async Task<List<RecruitmentDocumentDto>> GetRecruitmentDocumentsAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<RecruitmentDocumentDto>>($"api/admin/recruitment/{recruitmentId}/documents") ?? new();
    }

    public async Task<RecruitmentDocumentDto?> RequestDocumentAsync(Guid recruitmentId, RequestDocumentDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment/{recruitmentId}/documents", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecruitmentDocumentDto>();
    }

    public async Task<bool> UpdateDocumentStatusAsync(Guid documentId, UpdateDocumentStatusDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/documents/{documentId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/recruitment/documents/{documentId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMigrationInfoAsync(Guid recruitmentId, UpdateMigrationInfoDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/migration-info", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<VacancyOptionDto>> GetVacancyOptionsAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<VacancyOptionDto>>("api/admin/recruitment/vacancies") ?? new();
    }

    public async Task<bool> LinkVacancyAsync(Guid recruitmentId, LinkVacancyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment/{recruitmentId}/link-vacancy", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<DeliveryDto?> DeliverCandidateAsync(DeliverCandidateDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/recruitment-deliveries", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryDto>();
    }

    public async Task<List<DeliveryDto>> GetDeliveriesByRecruitmentAsync(Guid recruitmentId)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<DeliveryDto>>($"api/admin/recruitment-deliveries/recruitment/{recruitmentId}") ?? new();
    }

    public async Task<DeliveryDto?> SetDeliveryIncorporationDateAsync(Guid deliveryId, DateTime incorporationDate)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/incorporation", new SetIncorporationDateDto { IncorporationDate = incorporationDate });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryDto>();
    }

    public async Task<DeliveryDto?> SetDeliveryHiringDateAsync(Guid deliveryId, DateTime hiringDate)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/hiring-date", new SetHiringDateDto { HiringDate = hiringDate });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryDto>();
    }

    public async Task<(WarrantyReplacementDto? Result, string? Error)> ActivateDeliveryWarrantyReplacementAsync(Guid deliveryId, ActivateWarrantyReplacementDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/warranty-replacement", dto);
        return await ReadResultAsync<WarrantyReplacementDto>(response);
    }

    public async Task<CandidateDeliveryHistoryDto?> GetCandidateDeliveryHistoryAsync(Guid userId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/recruitment-deliveries/history/{userId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateDeliveryHistoryDto>();
    }

    public async Task<(bool Ok, string? Error)> ReleaseCandidateAsync(Guid userId, ReleaseCandidateDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment-deliveries/release-candidate/{userId}", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        var (_, error) = await ReadResultAsync<object>(response);
        return (false, error);
    }

    public async Task<(DeliveryDto? Result, string? Error)> RecordDeliveryCompanyResponseAsync(Guid deliveryId, RespondDeliveryDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/company-response", dto);
        return await ReadResultAsync<DeliveryDto>(response);
    }

    public async Task<(DeliveryDto? Result, string? Error)> CloseDeliveryProcessAsync(Guid deliveryId, CloseProcessDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/close-process", dto);
        return await ReadResultAsync<DeliveryDto>(response);
    }

    public async Task<(DeliveryDto? Result, string? Error)> RecordDeliveryFeedbackAsync(Guid deliveryId, RecordFeedbackDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/recruitment-deliveries/{deliveryId}/feedback", dto);
        return await ReadResultAsync<DeliveryDto>(response);
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
        await _localStorage.SetItemAsync("otwadmin-staff-role", auth.User.StaffRole?.ToString() ?? "");
    }

    public async Task<int?> GetStaffRoleAsync()
    {
        var value = await _localStorage.GetItemAsync("otwadmin-staff-role");
        return int.TryParse(value, out var role) ? role : null;
    }

    // --- Fase 3, sub-fase 3.8: Gestion de scores + Verificaciones manuales ---

    public async Task<CandidateScoreDto?> GetCandidateScoreAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/candidates/{candidateId}/score");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateScoreDto>();
    }

    public async Task<CandidateScoreDto?> RecalculateCandidateScoreAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/candidates/{candidateId}/score/recalculate", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CandidateScoreDto>();
    }

    public async Task<List<VerificationResultDto>> GetCandidateVerificationsAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/candidates/{candidateId}/verifications");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<VerificationResultDto>>() ?? new();
    }

    public async Task<bool> SetVerificationStatusAsync(Guid candidateId, int type, int status)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/candidates/{candidateId}/verifications/{type}", new { Status = status });
        return response.IsSuccessStatusCode;
    }

    public async Task<VerificationStatusDto?> GetVerificationStatusAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/candidates/{candidateId}/verification-status");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<VerificationStatusDto>();
    }

    public async Task<List<AdminApplicationDto>> GetCandidateApplicationsAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/candidates/{candidateId}/applications");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<AdminApplicationDto>>() ?? new();
    }

    public async Task<List<CandidateMatchDto>> GetCandidateMatchesAsync(Guid candidateId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/candidates/{candidateId}/matches");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<CandidateMatchDto>>() ?? new();
    }

    public async Task<List<SkillTestAdminDto>> GetSkillTestsAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/skill-tests");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<SkillTestAdminDto>>() ?? new();
    }

    public async Task<SkillTestAdminDto?> CreateSkillTestAsync(CreateSkillTestDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/skill-tests", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SkillTestAdminDto>();
    }

    public async Task<SkillTestAdminDto?> UpdateSkillTestAsync(Guid id, UpdateSkillTestDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/skill-tests/{id}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SkillTestAdminDto>();
    }

    public async Task<bool> DeleteSkillTestAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/skill-tests/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<int> CalculateVacancyMatchesAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/vacancies/{vacancyId}/matches/calculate", null);
        if (!response.IsSuccessStatusCode) return 0;
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        return doc.TryGetProperty("candidatesEvaluated", out var v) ? v.GetInt32() : 0;
    }

    public async Task<List<JobMatchDto>> GetVacancyShortlistAsync(Guid vacancyId, int? limit = null)
    {
        await SetAuthHeaderAsync();
        var url = $"api/admin/vacancies/{vacancyId}/matches" + (limit.HasValue ? $"?limit={limit}" : "");
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<JobMatchDto>>() ?? new();
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("otwadmin-token");
        await _localStorage.RemoveItemAsync("otwadmin-refresh-token");
        await _localStorage.RemoveItemAsync("otwadmin-user-id");
        await _localStorage.RemoveItemAsync("otwadmin-staff-role");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // --- Personal Administrativo (roles de staff) ---

    public async Task<List<StaffUserDto>> GetStaffAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/staff");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<StaffUserDto>>() ?? new();
    }

    public async Task<(bool Success, string? Error)> CreateStaffAsync(CreateStaffDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/staff", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<bool> ChangeStaffRoleAsync(Guid id, int staffRole)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/staff/{id}/role", new ChangeStaffRoleDto { StaffRole = staffRole });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActivateStaffAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/staff/{id}/activate", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeactivateStaffAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/staff/{id}/deactivate", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<ResetStaffPasswordResultDto?> ResetStaffPasswordAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/staff/{id}/reset-password", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResetStaffPasswordResultDto>();
    }

    // --- Negociaciones (Comercial) ---

    public async Task<NegotiationDto?> CreateNegotiationAsync(CreateNegotiationDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/negotiations", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NegotiationDto>();
    }

    public async Task<NegotiationDto?> CloseNegotiationAsync(Guid id, Guid winningApplicationId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/negotiations/{id}/close", new CloseNegotiationDto { WinningApplicationId = winningApplicationId });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NegotiationDto>();
    }

    public async Task<NegotiationDto?> UpdateNegotiationStatusAsync(Guid id, int status)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/negotiations/{id}/status", new UpdateNegotiationStatusDto { Status = status });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NegotiationDto>();
    }

    public async Task<NegotiationDto?> SetNegotiationIncorporationDateAsync(Guid negotiationId, DateTime incorporationDate)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/negotiations/{negotiationId}/incorporation", new SetIncorporationDateDto { IncorporationDate = incorporationDate });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NegotiationDto>();
    }

    public async Task<NegotiationDto?> SetNegotiationHiringDateAsync(Guid negotiationId, DateTime hiringDate)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/negotiations/{negotiationId}/hiring-date", new SetHiringDateDto { HiringDate = hiringDate });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<NegotiationDto>();
    }

    public async Task<List<NegotiationDto>> GetNegotiationsByVacancyAsync(Guid vacancyId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/negotiations/vacancy/{vacancyId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<NegotiationDto>>() ?? new();
    }

    public async Task<(WarrantyReplacementDto? Result, string? Error)> ActivateNegotiationWarrantyReplacementAsync(Guid negotiationId, ActivateWarrantyReplacementDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/negotiations/{negotiationId}/warranty-replacement", dto);
        return await ReadResultAsync<WarrantyReplacementDto>(response);
    }

    public async Task<(NegotiationDto? Result, string? Error)> CloseNegotiationProcessAsync(Guid negotiationId, CloseProcessDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/negotiations/{negotiationId}/close-process", dto);
        return await ReadResultAsync<NegotiationDto>(response);
    }

    public async Task<(NegotiationDto? Result, string? Error)> RecordNegotiationFeedbackAsync(Guid negotiationId, RecordFeedbackDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/negotiations/{negotiationId}/feedback", dto);
        return await ReadResultAsync<NegotiationDto>(response);
    }

    // --- Company CRM ---

    public async Task<List<CompanyListDto>> GetCompaniesAsync(string? search = null, int? status = null, Guid? assignedTo = null, int page = 1, int pageSize = 50)
    {
        await SetAuthHeaderAsync();
        var url = $"api/admin/company-crm/companies?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (status.HasValue) url += $"&status={status.Value}";
        if (assignedTo.HasValue) url += $"&assignedTo={assignedTo.Value}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<CompanyListDto>>() ?? new();
    }

    public async Task<CompanyDetailDto?> GetCompanyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/company-crm/companies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyDetailDto>();
    }

    public async Task<CompanyDetailDto?> CreateCompanyAsync(CreateCompanyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/company-crm/companies", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyDetailDto>();
    }

    public async Task<CompanyDetailDto?> UpdateCompanyAsync(Guid id, UpdateCompanyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/companies/{id}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyDetailDto>();
    }

    public async Task<bool> DeleteCompanyAsync(Guid id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/admin/company-crm/companies/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetCompanyFeaturedAsync(Guid id, bool featured)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/companies/{id}/featured", new { featured });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetCompanyPlanTierAsync(Guid id, CompanyPlanTier tier)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/companies/{id}/plan-tier", new { Tier = tier });
        return response.IsSuccessStatusCode;
    }

    public async Task<CompanyPipelineResultDto> GetCompanyPipelineAsync(int page = 1, int pageSize = 50, int? stage = null, Guid? assignedTo = null, string? search = null)
    {
        await SetAuthHeaderAsync();
        var url = $"api/admin/company-crm/pipeline?page={page}&pageSize={pageSize}";
        if (stage.HasValue) url += $"&stage={stage.Value}";
        if (assignedTo.HasValue) url += $"&assignedTo={assignedTo.Value}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<CompanyPipelineResultDto>() ?? new();
    }

    public async Task<CompanyPipelineDetailDto?> GetCompanyPipelineDetailAsync(Guid pipelineId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/admin/company-crm/pipeline/{pipelineId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyPipelineDetailDto>();
    }

    public async Task<CompanyPipelineDto?> AssignCompanyAsync(AssignCompanyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/admin/company-crm/assign", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyPipelineDto>();
    }

    public async Task<bool> MoveCompanyStageAsync(Guid pipelineId, CompanyMoveStageDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/pipeline/{pipelineId}/move-stage", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnassignCompanyAsync(Guid pipelineId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsync($"api/admin/company-crm/pipeline/{pipelineId}/unassign", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DismissCompanyAsync(Guid pipelineId, DismissCompanyDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/company-crm/pipeline/{pipelineId}/dismiss", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RestoreCompanyAsync(Guid pipelineId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/admin/company-crm/pipeline/{pipelineId}/restore", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReassignCompanyAsync(Guid pipelineId, Guid newUserId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/pipeline/{pipelineId}/reassign", new { newUserId });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCompanyPipelineNotesAsync(Guid pipelineId, UpdatePipelineNotesDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/admin/company-crm/pipeline/{pipelineId}/notes", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<PlanDto>> GetPlansAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/admin/company-crm/plans");
        return await response.Content.ReadFromJsonAsync<List<PlanDto>>() ?? new();
    }

    // --- Cartera de Clientes (Portfolio) ---

    public async Task<List<PortfolioSummaryDto>> GetPortfolioAsync(Guid? assignedTo = null)
    {
        await SetAuthHeaderAsync();
        var url = "api/admin/portfolio";
        if (assignedTo.HasValue) url += $"?assignedTo={assignedTo.Value}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<PortfolioSummaryDto>>() ?? new();
    }

    // --- Pagos centralizados ---

    public async Task<PaymentListResultDto?> GetPaymentsAsync(int? status = null, int page = 1, int pageSize = 20, string? search = null)
    {
        await SetAuthHeaderAsync();
        var url = $"api/admin/payments?page={page}&pageSize={pageSize}";
        if (status.HasValue) url += $"&status={status.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PaymentListResultDto>();
    }

    public async Task<ContractPaymentDto?> MarkPaymentPaidAsync(Guid trancheId, string? notes)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync($"api/admin/payments/{trancheId}/mark-paid", new MarkTranchePaidDto { Notes = notes });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ContractPaymentDto>();
    }
}
