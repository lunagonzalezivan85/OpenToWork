namespace OpenToWork.Shared.DTOs;

/// <summary>
/// Anexo de contrato por vacante (secciones 5/6/7). Solo editable en Draft;
/// Accepted/Rejected/Cancelled son de solo lectura.
/// </summary>
public class AdminVacancyContractDto
{
    public Guid Id { get; set; }
    public Guid VacancyId { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyContactName { get; set; }
    public string? CompanyContactPhone { get; set; }
    public string? VacancyTitle { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public int Status { get; set; }

    // 5. Alcance del servicio y plazos
    public List<string> ScopeServices { get; set; } = new();
    public int? TargetCandidates { get; set; }
    public int JobTypeCategory { get; set; }
    public int? TargetCoverageDays { get; set; }

    // 6. Garantia
    public int? WarrantyDays { get; set; }

    // 7. Condiciones economicas
    public decimal? FeeAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public int FeeApplicationType { get; set; }
    public decimal PaymentOpeningPct { get; set; } = 30m;
    public decimal PaymentValidationPct { get; set; } = 50m;
    public decimal PaymentConsolidationPct { get; set; } = 20m;
    public string? FeeExceptions { get; set; }

    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminSaveVacancyContractDto
{
    public List<string> ScopeServices { get; set; } = new();
    public int? TargetCandidates { get; set; }
    public int JobTypeCategory { get; set; }
    public int? TargetCoverageDays { get; set; }
    public int? WarrantyDays { get; set; }
    public decimal? FeeAmount { get; set; }
    public string? Currency { get; set; }
    public int FeeApplicationType { get; set; }
    public decimal PaymentOpeningPct { get; set; } = 30m;
    public decimal PaymentValidationPct { get; set; } = 50m;
    public decimal PaymentConsolidationPct { get; set; } = 20m;
    public string? FeeExceptions { get; set; }
}

public class AdminContractDecisionDto
{
    public bool Accepted { get; set; }

    /// <summary>Motivo obligatorio al rechazar.</summary>
    public string? Reason { get; set; }
}
