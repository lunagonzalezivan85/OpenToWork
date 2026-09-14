namespace OpenToWork.Shared.DTOs;

/// <summary>
/// Resumen de la cartera de un comercial: empresas asignadas, facturación total y tramos pendientes.
/// </summary>
public class PortfolioSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int StaffRole { get; set; }
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int WonCompanies { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalPending { get; set; }
    public List<PortfolioCompanyDto> Companies { get; set; } = new();
}

public class PortfolioCompanyDto
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? City { get; set; }
    public int CurrentStage { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public int VacancyCount { get; set; }
    public int ContractCount { get; set; }
    public decimal? ContractTotal { get; set; }
    public decimal? PendingTotal { get; set; }
}

/// <summary>
/// Tramo de pago con contexto del contrato y empresa, para la vista centralizada de pagos.
/// </summary>
public class PaymentListItemDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int TrancheType { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaidByName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentListResultDto
{
    public List<PaymentListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
}
