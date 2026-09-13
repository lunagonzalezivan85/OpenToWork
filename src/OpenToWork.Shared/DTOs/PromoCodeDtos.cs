namespace OpenToWork.Shared.DTOs;

public class PromoCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public Guid? PT_JobLevelId { get; set; }
    public string? JobLevelName { get; set; }
    public Guid? PT_JobTypeId { get; set; }
    public string? JobTypeName { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int UsesCount { get; set; }
    public bool IsActive { get; set; }
}

public class SavePromoCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public Guid? PT_JobLevelId { get; set; }
    public Guid? PT_JobTypeId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Resultado de validar un codigo promocional contra una vacante/monto base concreto (preview, sin consumir el uso).</summary>
public class PromoValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? PromoCodeId { get; set; }
    public string? Code { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class ValidatePromoCodeDto
{
    public string Code { get; set; } = string.Empty;
    public Guid VacancyId { get; set; }
}
