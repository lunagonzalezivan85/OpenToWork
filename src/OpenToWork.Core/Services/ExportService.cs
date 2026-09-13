using System.Globalization;
using System.Text;
using OpenToWork.Core.Interfaces;

namespace OpenToWork.Core.Services;

public class ExportService : IExportService
{
    private const int ExportPageSize = 100_000;

    private readonly IAdminUserService _userService;
    private readonly IAdminVacancyService _vacancyService;

    public ExportService(IAdminUserService userService, IAdminVacancyService vacancyService)
    {
        _userService = userService;
        _vacancyService = vacancyService;
    }

    public async Task<string> ExportUsersCsvAsync()
    {
        var users = await _userService.GetUsersAsync(1, ExportPageSize, role: null, isActive: null);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Email,PrimaryRole,EmailVerified,IsActive,CreatedAt,LastLoginAt");
        foreach (var u in users)
        {
            sb.AppendLine(string.Join(",",
                u.Id,
                CsvField(u.Email),
                u.PrimaryRole,
                u.EmailVerified,
                u.IsActive,
                u.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
                u.LastLoginAt?.ToString("O", CultureInfo.InvariantCulture) ?? ""));
        }

        return sb.ToString();
    }

    public async Task<string> ExportVacanciesCsvAsync()
    {
        var vacancies = await _vacancyService.GetVacanciesAsync(1, ExportPageSize, status: null);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Title,CompanyName,Location,ContractType,WorkMode,Status,IsTemporary,PublishedAt,ClosedAt,ExpiresAt,ViewsCount");
        foreach (var v in vacancies.Items)
        {
            sb.AppendLine(string.Join(",",
                v.Id,
                CsvField(v.Title),
                CsvField(v.CompanyName ?? ""),
                CsvField(v.Location ?? ""),
                v.ContractType,
                v.WorkMode,
                v.Status,
                v.IsTemporary,
                v.PublishedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "",
                v.ClosedAt?.ToString("O", CultureInfo.InvariantCulture) ?? "",
                v.ExpiresAt?.ToString("O", CultureInfo.InvariantCulture) ?? "",
                v.ViewsCount));
        }

        return sb.ToString();
    }

    private static string CsvField(string value)
    {
        if (value.Length > 0 && (value[0] == '=' || value[0] == '+' || value[0] == '-' || value[0] == '@'))
            value = "'" + value;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
