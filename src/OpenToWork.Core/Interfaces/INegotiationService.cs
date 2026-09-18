using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface INegotiationService
{
    Task<NegotiationDto?> CreateAsync(CreateNegotiationDto dto, Guid staffId);
    Task<NegotiationDto?> UpdateStatusAsync(Guid id, int status);
    Task<NegotiationDto?> CloseAsync(Guid id, Guid winningApplicationId, Guid staffId, string? ipAddress);
    Task<NegotiationDto?> SetIncorporationDateAsync(Guid id, DateTime incorporationDate, Guid staffId);
    Task<NegotiationDto?> SetHiringDateAsync(Guid id, DateTime hiringDate, Guid staffId);
    Task<NegotiationDto?> CloseProcessAsync(Guid id, CloseProcessDto dto, Guid staffId);
    Task<NegotiationDto?> RecordFeedbackAsync(Guid id, RecordFeedbackDto dto, Guid staffId);
    Task<List<NegotiationDto>> GetByVacancyAsync(Guid vacancyId);
}
