using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryDto?> DeliverCandidateAsync(DeliverCandidateDto dto, Guid adminId, string? ipAddress);
    Task<List<DeliveryDto>> GetDeliveriesByRecruitmentAsync(Guid recruitmentId);
    Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid companyUserId, Guid? vacancyId);
    Task<VacancyApplicantSummaryDto?> GetVacancySummaryAsync(Guid vacancyId, Guid companyUserId);
    Task<DeliveryDto?> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback, int? rejectionReason, Guid companyUserId);
    Task<DeliveryDto?> RecordCompanyResponseByAdminAsync(Guid deliveryId, int status, string? feedback, int? rejectionReason, Guid adminId, string? ipAddress);
    /// <summary>"Liberar candidato": termina todas sus colocaciones activas (entregas y negociaciones) cuando deja el puesto fuera de garantia. Devuelve cuantas termino.</summary>
    Task<int> ReleaseCandidateAsync(Guid userId, ReleaseCandidateDto dto, Guid adminId, string? ipAddress);
    /// <summary>Historial de entregas del candidato (todas las empresas) con rechazos por motivo y aviso de "quemado".</summary>
    Task<CandidateDeliveryHistoryDto?> GetCandidateHistoryAsync(Guid userId);
    Task<DeliveryDto?> SetIncorporationDateAsync(Guid deliveryId, DateTime incorporationDate, Guid adminId);
    Task<DeliveryDto?> SetHiringDateAsync(Guid deliveryId, DateTime hiringDate, Guid adminId);
    Task<DeliveryDto?> CloseProcessAsync(Guid deliveryId, CloseProcessDto dto, Guid adminId);
    Task<DeliveryDto?> RecordFeedbackAsync(Guid deliveryId, RecordFeedbackDto dto, Guid adminId);

    /// <summary>Entregas Contratadas de una vacante, para vincular una reposicion de garantia
    /// (paso 20) desde el panel de contrato.</summary>
    Task<List<DeliveryDto>> GetHiredDeliveriesByVacancyAsync(Guid vacancyId);
}
