using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenToWork.Core.Interfaces;
using OpenToWork.Core.Services;
using OpenToWork.Models.Context;

namespace OpenToWork.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenCryptoService, TokenCryptoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IVacancyService, VacancyService>();
        services.AddScoped<IPermanentVacancyService, PermanentVacancyService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ICvParserService, CvParserService>();
        services.AddHttpClient<ICvParserService, CvParserService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddHttpClient<IValidationService, ValidationService>();
        services.AddScoped<IScoringService, ScoringService>();

        return services;
    }

    public static IServiceCollection AddAdminCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenCryptoService, TokenCryptoService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminVacancyService, AdminVacancyService>();
        services.AddScoped<IAdminSkillService, AdminSkillService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IAdminApplicationService, AdminApplicationService>();
        services.AddScoped<IAdminCandidateService, AdminCandidateService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();

        return services;
    }

    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        return services;
    }
}
