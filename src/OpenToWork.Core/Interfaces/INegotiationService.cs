using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface INegotiationService
{
    Task<NegotiationDto?> CreateAsync(CreateNegotiationDto dto, Guid staffId);
    Task<NegotiationDto?> UpdateStatusAsync(Guid id, int status);
    Task<NegotiationDto?> CloseAsync(Guid id, Guid winningApplicationId, Guid staffId, string? ipAddress);
    Task<List<NegotiationDto>> GetByVacancyAsync(Guid vacancyId);
}
