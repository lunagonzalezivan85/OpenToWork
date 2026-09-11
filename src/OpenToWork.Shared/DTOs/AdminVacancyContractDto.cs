namespace OpenToWork.Shared.DTOs;

/// <summary>
/// Anexo de contrato de servicio por empresa (secciones 5/6/7). Un contrato agrupa
/// N vacantes de la misma empresa. Solo editable en Draft;
/// Accepted/Rejected/Cancelled son de solo lectura.
/// </summary>
public class AdminVacancyContractDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyContactName { get; set; }
    public string? CompanyContactPhone { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public int Status { get; set; }

    // Vacantes incluidas en el contrato (1:N)
    public List<ContractVacancyItemDto> Vacancies { get; set; } = new();

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
    public List<Guid> VacancyIds { get; set; } = new();
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

/// <summary>Vacante incluida en un contrato.</summary>
public class ContractVacancyItemDto
{
    public Guid VacancyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int? RequiredApplicants { get; set; }
}
