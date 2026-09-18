using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenToWork.AdminAPI.Authorization;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Controllers;

[Route("api/admin")]
[RequireStaffRole(AdminStaffRole.Comercial)]
public class PortfolioPaymentController : AdminControllerBase
{
    private readonly AppDbContext _db;
    private readonly IContractPaymentService _paymentService;

    public PortfolioPaymentController(AppDbContext db, IContractPaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    /// <summary>Cartera de clientes: empresas asignadas a cada comercial, con facturación y tramos pendientes.</summary>
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetPortfolio([FromQuery] Guid? assignedTo = null)
    {
        var pipelines = _db.PT_CompanyPipelines
            .Include(p => p.Company)
                .ThenInclude(c => c.Vacancies)
            .Include(p => p.AssignedToUser)
            .Where(p => !p.IsDeleted && !p.IsDismissed && p.AssignedToUserId.HasValue);

        if (assignedTo.HasValue)
            pipelines = pipelines.Where(p => p.AssignedToUserId == assignedTo.Value);

        var pipelineList = await pipelines.ToListAsync();

        var companyIds = pipelineList.Select(p => p.PT_CompanyId).Distinct().ToList();

        var contracts = await _db.PT_VacancyContracts
            .Where(c => companyIds.Contains(c.PT_CompanyId) && !c.IsDeleted)
            .ToListAsync();

        var contractIds = contracts.Select(c => c.Id).ToList();

        var payments = await _db.PT_ContractPayments
            .Where(p => contractIds.Contains(p.PT_VacancyContractId) && !p.IsDeleted)
            .ToListAsync();

        var grouped = pipelineList
            .GroupBy(p => p.AssignedToUserId!.Value)
            .Select(g =>
            {
                var user = g.First().AssignedToUser!;
                var companyIdsForUser = g.Select(p => p.PT_CompanyId).Distinct().ToList();
                var contractsForUser = contracts.Where(c => companyIdsForUser.Contains(c.PT_CompanyId)).ToList();
                var contractIdsForUser = contractsForUser.Select(c => c.Id).ToList();
                var paymentsForUser = payments.Where(p => contractIdsForUser.Contains(p.PT_VacancyContractId)).ToList();

                var companyDtos = g.Select(p => new PortfolioCompanyDto
                {
                    CompanyId = p.PT_CompanyId,
                    CompanyName = p.Company.Name,
                    Industry = p.Company.Industry,
                    City = p.Company.City,
                    CurrentStage = p.CurrentStage,
                    AssignedToName = user.Email,
                    AssignedToUserId = p.AssignedToUserId,
                    AssignedAt = p.AssignedAt,
                    VacancyCount = p.Company.Vacancies.Count(v => !v.IsDeleted),
                    ContractCount = contractsForUser.Count(c => c.PT_CompanyId == p.PT_CompanyId),
                    ContractTotal = contractsForUser
                        .Where(c => c.PT_CompanyId == p.PT_CompanyId && c.FeeAmount.HasValue)
                        .Sum(c => c.FeeAmount!.Value),
                    PendingTotal = paymentsForUser
                        .Where(pm => contractsForUser.Any(c => c.Id == pm.PT_VacancyContractId && c.PT_CompanyId == p.PT_CompanyId))
                        .Where(pm => pm.Status == (int)PaymentTrancheStatus.Pendiente)
                        .Sum(pm => pm.Amount)
                }).ToList();

                return new PortfolioSummaryDto
                {
                    UserId = g.Key,
                    UserName = user.Email,
                    StaffRole = user.StaffRole ?? 0,
                    TotalCompanies = companyDtos.Count,
                    ActiveCompanies = companyDtos.Count(c => c.CurrentStage < (int)CompanyPipelineStage.CerradoGanado),
                    WonCompanies = companyDtos.Count(c => c.CurrentStage == (int)CompanyPipelineStage.CerradoGanado),
                    TotalBilled = paymentsForUser.Sum(pm => pm.Amount),
                    TotalPending = paymentsForUser.Where(pm => pm.Status == (int)PaymentTrancheStatus.Pendiente).Sum(pm => pm.Amount),
                    Companies = companyDtos
                };
            }).ToList();

        return Ok(grouped);
    }

    /// <summary>Lista centralizada de todos los tramos de pago, con filtro por estado.</summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] int? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var query = _db.PT_ContractPayments
            .Include(p => p.Contract)
                .ThenInclude(c => c != null ? c.Company : null!)
            .Include(p => p.PaidByUser)
            .Where(p => !p.IsDeleted);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Contract != null &&
                (p.Contract.ContractNumber.Contains(search) ||
                 (p.Contract.Company != null && p.Contract.Company.Name.Contains(search))));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentListItemDto
            {
                Id = p.Id,
                ContractId = p.PT_VacancyContractId,
                ContractNumber = p.Contract != null ? p.Contract.ContractNumber : "",
                CompanyId = p.Contract != null ? p.Contract.PT_CompanyId : Guid.Empty,
                CompanyName = p.Contract != null && p.Contract.Company != null ? p.Contract.Company.Name : "",
                TrancheType = p.TrancheType,
                Percentage = p.Percentage,
                Amount = p.Amount,
                Status = p.Status,
                PaidAt = p.PaidAt,
                PaidByName = p.PaidByUser != null ? p.PaidByUser.Email : null,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new PaymentListResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalAmount = await query.SumAsync(p => p.Amount),
            TotalPaid = await query.Where(p => p.Status == (int)PaymentTrancheStatus.Pagado).SumAsync(p => p.Amount),
            TotalPending = await query.Where(p => p.Status == (int)PaymentTrancheStatus.Pendiente).SumAsync(p => p.Amount)
        };

        return Ok(result);
    }

    /// <summary>Marca un tramo de pago como pagado (mismo endpoint que VacancyContractController, replicado para la vista centralizada).</summary>
    [HttpPost("payments/{trancheId:guid}/mark-paid")]
    public async Task<IActionResult> MarkTranchePaid(Guid trancheId, [FromBody] MarkTranchePaidDto dto)
    {
        var result = await _paymentService.MarkAsPaidAsync(trancheId, dto, AdminId);
        return result == null ? NotFound() : Ok(result);
    }
}
