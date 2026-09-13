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
    public string? CompanyLegalName { get; set; }
    public string? CompanyTaxId { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyCountry { get; set; }
    public string? CompanyCity { get; set; }
    public string? CompanyContactName { get; set; }
    public string? CompanyContactPosition { get; set; }
    public string? CompanyContactDniNie { get; set; }
    public string? CompanyContactEmail { get; set; }
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

    // 7. Condiciones economicas. FeeAmount es la suma de FinalPrice de cada linea (Vacancies) - no se edita directo.
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
    /// <summary>Una linea por vacante, cada una con su propio precio/promo (ver ContractVacancyLineDto).</summary>
    public List<ContractVacancyLineDto> VacancyLines { get; set; } = new();
    public List<string> ScopeServices { get; set; } = new();
    public int? TargetCandidates { get; set; }
    public int JobTypeCategory { get; set; }
    public int? TargetCoverageDays { get; set; }
    public int? WarrantyDays { get; set; }
    public string? Currency { get; set; }
    public int FeeApplicationType { get; set; }
    public decimal PaymentOpeningPct { get; set; } = 30m;
    public decimal PaymentValidationPct { get; set; } = 50m;
    public decimal PaymentConsolidationPct { get; set; } = 20m;
    public string? FeeExceptions { get; set; }
}

/// <summary>Una vacante dentro del contrato, con su precio (automatico por lista + promo, o manual).</summary>
public class ContractVacancyLineDto
{
    public Guid VacancyId { get; set; }

    /// <summary>Codigo promocional a aplicar sobre el precio de lista. Ignorado si ManualPrice tiene valor.</summary>
    public string? PromoCode { get; set; }

    /// <summary>Si se informa, reemplaza el precio de lista por completo (negociacion puntual). Requiere OverrideReason.</summary>
    public decimal? ManualPrice { get; set; }

    public string? OverrideReason { get; set; }
}

public class AdminContractDecisionDto
{
    public bool Accepted { get; set; }

    /// <summary>Motivo obligatorio al rechazar.</summary>
    public string? Reason { get; set; }
}

/// <summary>Vacante incluida en un contrato, con el desglose de precio de esa linea.</summary>
public class ContractVacancyItemDto
{
    public Guid VacancyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Location { get; set; }
    public string? Category { get; set; }
    public int ContractType { get; set; }
    public int WorkMode { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int? RequiredApplicants { get; set; }
    public int? YearsExperience { get; set; }

    // Desglose de precio de esta linea (snapshot al generar el contrato)
    public Guid? JobTypeId { get; set; }
    public string? JobTypeName { get; set; }
    public string? JobLevelName { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PromoCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? FinalPrice { get; set; }
    public bool IsManualOverride { get; set; }
    public string? OverrideReason { get; set; }
}

/// <summary>Tramo de pago del anexo (Apertura/Validacion/Consolidacion). Se generan los 3 al
/// aceptar el contrato; un admin los marca pagado/pendiente a mano (sin pasarela integrada).</summary>
public class ContractPaymentDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }

    /// <summary>PaymentTrancheType: Apertura=0 / Validacion=1 / Consolidacion=2.</summary>
    public int TrancheType { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }

    /// <summary>PaymentTrancheStatus: Pendiente=0 / Pagado=1.</summary>
    public int Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaidByName { get; set; }
    public string? Notes { get; set; }
}

public class MarkTranchePaidDto
{
    public string? Notes { get; set; }
}
