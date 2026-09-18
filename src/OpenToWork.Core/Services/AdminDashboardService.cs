using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync()
    {
        var vacanciesByStatus = await _context.PT_Vacancies
            .Where(v => !v.IsDeleted)
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var applicationsByStatus = await _context.PT_Applications
            .Where(a => !a.IsDeleted)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var companiesWithVacancies = await _context.PT_Vacancies
            .Where(v => !v.IsDeleted)
            .Select(v => v.PT_CompanyId)
            .Distinct()
            .CountAsync();

        return new DashboardMetricsDto
        {
            TotalUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted),
            ActiveUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.IsActive),
            TotalCandidates = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted),
            TotalCompanies = await _context.PT_Companies.CountAsync(c => !c.IsDeleted),
            TotalPermanentVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted),
            TotalTempVacancies = await _context.PT_TempVacancies.CountAsync(v => !v.IsDeleted),
            VacanciesByStatus = vacanciesByStatus.ToDictionary(x => ((VacancyStatus)x.Status).ToString(), x => x.Count),
            ApplicationsByStatus = applicationsByStatus.ToDictionary(x => ((ApplicationStatus)x.Status).ToString(), x => x.Count),
            TotalSkills = await _context.PT_Skills.CountAsync(s => !s.IsDeleted),
            TotalAuditLogEntries = await _context.AD_AuditLogs.CountAsync(a => !a.IsDeleted),

            EvaluatedProfiles = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && c.WizardCompleted),
            PendingProfiles = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !c.WizardCompleted),
            ProfilesWithScores = 0,
            OpenVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Active),
            ClosedVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Closed),
            DraftVacancies = await _context.PT_Vacancies.CountAsync(v => !v.IsDeleted && v.Status == (int)VacancyStatus.Draft),
            CompaniesWithVacancies = companiesWithVacancies,
            CompaniesWithoutVacancies = await _context.PT_Companies.CountAsync(c => !c.IsDeleted) - companiesWithVacancies,
            NonAdminUsers = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole != (int)UserRole.Admin),
            NonAdminCandidates = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole == (int)UserRole.Candidate),
            NonAdminCompanies = await _context.SC_Users.CountAsync(u => !u.IsDeleted && u.PrimaryRole == (int)UserRole.Company),
            CandidatesWithLinkedIn = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.LinkedInUrl)),
            CandidatesWithPortfolio = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.PortfolioUrl)),
            CandidatesWithCV = await _context.PT_Candidates.CountAsync(c => !c.IsDeleted && !string.IsNullOrEmpty(c.CvUrl))
        };
    }

    public async Task<BusinessMetricsDto> GetBusinessMetricsAsync()
    {
        // 1. Ingresos: cobrado vs pendiente sobre los tramos ya generados
        var paymentsByStatus = await _context.PT_ContractPayments
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();
        var revenueCollected = paymentsByStatus.FirstOrDefault(x => x.Status == (int)PaymentTrancheStatus.Pagado)?.Total ?? 0m;
        var revenuePending = paymentsByStatus.FirstOrDefault(x => x.Status == (int)PaymentTrancheStatus.Pendiente)?.Total ?? 0m;

        // 2. Tasa de cierre comercial
        var dealsWon = await _context.PT_CompanyPipelines.CountAsync(p => !p.IsDeleted && !p.IsDismissed && p.CurrentStage == (int)CompanyPipelineStage.CerradoGanado);
        var dealsLost = await _context.PT_CompanyPipelines.CountAsync(p => !p.IsDeleted && !p.IsDismissed && p.CurrentStage == (int)CompanyPipelineStage.CerradoPerdido);
        var winRate = (dealsWon + dealsLost) > 0 ? (double)dealsWon / (dealsWon + dealsLost) * 100 : 0;

        // 3. Tiempo promedio de contratacion (dias): publicacion de la vacante -> HiringDate
        var negotiationDurations = await _context.PT_Negotiations
            .Where(n => !n.IsDeleted && n.Status == (int)NegotiationStatus.Cerrada && n.HiringDate != null)
            .Select(n => new { Start = n.Vacancy.PublishedAt ?? n.Vacancy.CreatedAt, End = n.HiringDate!.Value })
            .ToListAsync();
        var deliveryDurations = await _context.PT_CandidateDeliveries
            .Where(d => !d.IsDeleted && d.Status == (int)DeliveryStatus.Hired && d.HiringDate != null)
            .Select(d => new { Start = d.Vacancy.PublishedAt ?? d.Vacancy.CreatedAt, End = d.HiringDate!.Value })
            .ToListAsync();
        var hireDurationsDays = negotiationDurations.Concat(deliveryDurations)
            .Select(x => (x.End - x.Start).TotalDays)
            .Where(days => days >= 0)
            .ToList();
        var avgTimeToHire = hireDurationsDays.Count > 0 ? hireDurationsDays.Average() : (double?)null;

        // 4. Tasa de exito de colocaciones: 100% - % de contrataciones con reposicion no excluida
        var totalHires = await _context.PT_Negotiations.CountAsync(n => !n.IsDeleted && n.Status == (int)NegotiationStatus.Cerrada)
            + await _context.PT_CandidateDeliveries.CountAsync(d => !d.IsDeleted && d.Status == (int)DeliveryStatus.Hired);
        var coveredReplacements = await _context.PT_WarrantyReplacements
            .Where(w => !w.IsDeleted && !w.IsExclusion)
            .Select(w => new { w.OriginalNegotiationId, w.OriginalDeliveryId })
            .ToListAsync();
        var placementsWithIssue = coveredReplacements
            .Select(w => w.OriginalNegotiationId ?? w.OriginalDeliveryId)
            .Where(id => id != null)
            .Distinct()
            .Count();
        var successRate = totalHires > 0 ? (double)(totalHires - placementsWithIssue) / totalHires * 100 : 100.0;

        // 5. Valor potencial en contratos todavia no aceptados (Draft/Sent)
        var openPipelineValue = await _context.PT_VacancyContracts
            .Where(c => !c.IsDeleted && (c.Status == (int)ContractStatus.Draft || c.Status == (int)ContractStatus.Sent))
            .SumAsync(c => c.FeeAmount ?? 0m);
        var openContractsCount = await _context.PT_VacancyContracts
            .CountAsync(c => !c.IsDeleted && (c.Status == (int)ContractStatus.Draft || c.Status == (int)ContractStatus.Sent));

        // 6. Empresas en pipeline abierto sin cambio de etapa hace 30+ dias
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var stalledCompanies = await _context.PT_CompanyPipelines
            .Where(p => !p.IsDeleted && !p.IsDismissed
                && p.CurrentStage != (int)CompanyPipelineStage.CerradoGanado
                && p.CurrentStage != (int)CompanyPipelineStage.CerradoPerdido)
            .CountAsync(p => (p.StageEnteredAt ?? p.UpdatedAt ?? p.CreatedAt) < cutoff);

        return new BusinessMetricsDto
        {
            RevenueCollected = revenueCollected,
            RevenuePending = revenuePending,
            DealsWon = dealsWon,
            DealsLost = dealsLost,
            WinRatePercentage = winRate,
            AverageTimeToHireDays = avgTimeToHire,
            TimeToHireSampleSize = hireDurationsDays.Count,
            TotalHires = totalHires,
            PlacementsWithWarrantyIssue = placementsWithIssue,
            PlacementSuccessRatePercentage = successRate,
            OpenPipelineValue = openPipelineValue,
            OpenContractsCount = openContractsCount,
            StalledCompaniesCount = stalledCompanies
        };
    }
}
