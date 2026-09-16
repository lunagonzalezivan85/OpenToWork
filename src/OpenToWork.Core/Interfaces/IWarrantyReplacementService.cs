using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IWarrantyReplacementService
{
    Task<WarrantyReplacementDto?> ActivateFromNegotiationAsync(Guid negotiationId, ActivateWarrantyReplacementDto dto, Guid staffId, string? ipAddress);

    Task<WarrantyReplacementDto?> ActivateFromDeliveryAsync(Guid deliveryId, ActivateWarrantyReplacementDto dto, Guid staffId, string? ipAddress);

    Task<WarrantyReplacementDto?> LinkReplacementAsync(Guid replacementId, LinkWarrantyReplacementDto dto, Guid staffId);

    Task<WarrantyReplacementDto?> CancelAsync(Guid replacementId, Guid staffId);

    Task<List<WarrantyReplacementDto>> GetByContractAsync(Guid contractId);
}
