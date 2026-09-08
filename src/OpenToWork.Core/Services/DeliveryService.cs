using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class DeliveryService : IDeliveryService
{
    private readonly AppDbContext _context;
    private readonly IVerificationStatusService _verificationStatus;
    private readonly IAuditLogService _auditLog;

    public DeliveryService(AppDbContext context, IVerificationStatusService verificationStatus, IAuditLogService auditLog)
    {
        _context = context;
        _verificationStatus = verificationStatus;
        _auditLog = auditLog;
    }

    public async Task<DeliveryDto?> DeliverCandidateAsync(DeliverCandidateDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == dto.RecruitmentId && !r.IsDeleted);
        if (recruitment == null) return null;

        if (recruitment.CurrentStage != (int)RecruitmentStage.ReadyToDeliver)
            throw new InvalidOperationException("El candidato debe estar en la etapa Listo a Entregar.");

        var candidate = await _context.PT_Candidates
            .FirstOrDefaultAsync(c => c.SCUserId == recruitment.SCUserId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Perfil de candidato no encontrado.");

        var verification = await _verificationStatus.GetVerificationStatusAsync(candidate.Id);
        if (!verification.IsVerifiedTD)
            throw new InvalidOperationException("El candidato debe estar Verificado TD antes de ser entregado.");

        var vacancy = await _context.PT_Vacancies
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == dto.VacancyId && !v.IsDeleted);
        if (vacancy == null) throw new InvalidOperationException("Vacante no encontrada.");
        if (vacancy.Company == null) throw new InvalidOperationException("La vacante no tiene empresa asociada.");

        var alreadyDelivered = await _context.PT_CandidateDeliveries
            .AnyAsync(d => d.PT_CandidateRecruitmentId == dto.RecruitmentId
                && d.PT_VacancyId == dto.VacancyId && !d.IsDeleted);
        if (alreadyDelivered)
            throw new InvalidOperationException("El candidato ya fue entregado a esta vacante.");

        var delivery = new PTCandidateDelivery
        {
            Id = Guid.NewGuid(),
            PT_CandidateRecruitmentId = dto.RecruitmentId,
            PT_CandidateId = candidate.Id,
            PT_VacancyId = dto.VacancyId,
            PT_CompanyId = vacancy.Company.Id,
            DeliveredByUserId = adminId,
            DeliveredAt = DateTime.UtcNow,
            Status = (int)DeliveryStatus.Delivered,
            AdminNote = dto.AdminNote,
            CreatedAt = DateTime.UtcNow
        };

        _context.PT_CandidateDeliveries.Add(delivery);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "Recruitment.Deliver", "PTCandidateDelivery", delivery.Id,
            $"Candidato entregado a empresa {vacancy.Company.Name} para vacante {vacancy.Title}", ipAddress);

        return await MapToDtoAsync(delivery);
    }

    public async Task<List<DeliveryDto>> GetDeliveriesByRecruitmentAsync(Guid recruitmentId)
    {
        var deliveries = await _context.PT_CandidateDeliveries
            .Include(d => d.Candidate)
            .Include(d => d.Vacancy)
            .Include(d => d.Company)
            .Where(d => d.PT_CandidateRecruitmentId == recruitmentId && !d.IsDeleted)
            .OrderByDescending(d => d.DeliveredAt)
            .ToListAsync();

        var dtos = new List<DeliveryDto>();
        foreach (var d in deliveries)
            dtos.Add(await MapToDtoAsync(d));
        return dtos;
    }

    public async Task<List<DeliveryDto>> GetMyDeliveriesAsync(Guid companyUserId, Guid? vacancyId)
    {
        var query = _context.PT_CandidateDeliveries
            .Include(d => d.Candidate)
            .Include(d => d.Vacancy)
            .Include(d => d.Company)
            .Where(d => d.Company.SCUserId == companyUserId && !d.IsDeleted);

        if (vacancyId.HasValue)
            query = query.Where(d => d.PT_VacancyId == vacancyId.Value);

        var deliveries = await query
            .OrderByDescending(d => d.DeliveredAt)
            .ToListAsync();

        var dtos = new List<DeliveryDto>();
        foreach (var d in deliveries)
            dtos.Add(await MapToDtoAsync(d));
        return dtos;
    }

    public async Task<VacancyApplicantSummaryDto?> GetVacancySummaryAsync(Guid vacancyId, Guid companyUserId)
    {
        var vacancy = await _context.PT_Vacancies
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (vacancy == null || vacancy.Company?.SCUserId != companyUserId)
            return null;

        var total = await _context.PT_Applications
            .CountAsync(a => a.PT_VacancyId == vacancyId && !a.IsDeleted);

        var delivered = await _context.PT_CandidateDeliveries
            .CountAsync(d => d.PT_VacancyId == vacancyId && !d.IsDeleted);

        return new VacancyApplicantSummaryDto
        {
            VacancyId = vacancyId,
            Total = total,
            InVerification = total - delivered,
            Delivered = delivered
        };
    }

    public async Task<DeliveryDto?> RespondToDeliveryAsync(Guid deliveryId, int status, string? feedback, Guid companyUserId)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .Include(d => d.Company)
            .Include(d => d.Candidate)
            .Include(d => d.Vacancy)
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.Company?.SCUserId != companyUserId) return null;

        var allowed = new[] { (int)DeliveryStatus.Interested, (int)DeliveryStatus.Hired, (int)DeliveryStatus.RejectedByCompany };
        if (!allowed.Contains(status))
            throw new InvalidOperationException("Estado no permitido.");

        if (delivery.ViewedAt == null)
            delivery.ViewedAt = DateTime.UtcNow;

        delivery.Status = status;
        delivery.CompanyFeedback = feedback;
        delivery.RespondedAt = DateTime.UtcNow;
        delivery.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(delivery);
    }

    private async Task<DeliveryDto> MapToDtoAsync(PTCandidateDelivery d)
    {
        var verification = await _verificationStatus.GetVerificationStatusAsync(d.PT_CandidateId);

        return new DeliveryDto
        {
            Id = d.Id,
            CandidateId = d.PT_CandidateId,
            CandidateName = $"{d.Candidate.FirstName} {d.Candidate.LastName}".Trim(),
            CandidateTitle = d.Candidate.Title,
            VacancyId = d.PT_VacancyId,
            VacancyTitle = d.Vacancy?.Title ?? string.Empty,
            CompanyId = d.PT_CompanyId,
            CompanyName = d.Company?.Name ?? string.Empty,
            Status = d.Status,
            AdminNote = d.AdminNote,
            CompanyFeedback = d.CompanyFeedback,
            DeliveredAt = d.DeliveredAt,
            ViewedAt = d.ViewedAt,
            RespondedAt = d.RespondedAt,
            OverallScore = verification.OverallScore,
            ProfileCompletionPercentage = verification.ProfileCompletionPercentage,
            IsVerifiedTD = verification.IsVerifiedTD
        };
    }
}
