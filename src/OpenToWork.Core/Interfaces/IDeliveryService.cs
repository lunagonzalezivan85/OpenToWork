using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryDto?> DeliverCandidateAsync(DeliverCandidateDto dto, Guid adminId, string? ipAddress);
    Task<List<DeliveryDto>> GetDeliveriesByRecruitmentAsync(Guid recruitmentId);
    Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid companyUserId, Guid? vacancyId);
    Task<VacancyApplicantSummaryDto?> GetVacancySummaryAsync(Guid vacancyId, Guid companyUserId);
    Task<DeliveryDto?> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback, Guid companyUserId);
}
