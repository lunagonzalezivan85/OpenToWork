namespace OpenToWork.Shared.DTOs;

public class ActivateWarrantyReplacementDto
{
    public int Reason { get; set; }
    public string? Notes { get; set; }
}

public class LinkWarrantyReplacementDto
{
    public Guid? NegotiationId { get; set; }
    public Guid? DeliveryId { get; set; }
}

public class WarrantyReplacementCandidatesDto
{
    public List<NegotiationDto> Negotiations { get; set; } = new();
    public List<DeliveryDto> Deliveries { get; set; } = new();
}

public class WarrantyReplacementDto
{
    public Guid Id { get; set; }
    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = "";
    public Guid ContractId { get; set; }
    public Guid? OriginalNegotiationId { get; set; }
    public Guid? OriginalDeliveryId { get; set; }
    public string? OriginalCandidateName { get; set; }
    public int Reason { get; set; }
    public string? Notes { get; set; }
    public bool IsExclusion { get; set; }
    public int ReplacementNumber { get; set; }
    public int Status { get; set; }
    public string RequestedByName { get; set; } = "";
    public DateTime RequestedAt { get; set; }
    public Guid? ReplacementNegotiationId { get; set; }
    public Guid? ReplacementDeliveryId { get; set; }
    public string? ReplacementCandidateName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? ChargeTrancheId { get; set; }
    public decimal? ChargeAmount { get; set; }
    public int? ChargeStatus { get; set; }
}
