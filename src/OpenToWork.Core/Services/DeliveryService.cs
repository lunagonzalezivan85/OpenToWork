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
    private readonly IWarrantyLookupService _warranty;

    public DeliveryService(AppDbContext context, IVerificationStatusService verificationStatus, IAuditLogService auditLog, IWarrantyLookupService warranty)
    {
        _context = context;
        _verificationStatus = verificationStatus;
        _auditLog = auditLog;
        _warranty = warranty;
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

    public async Task<DeliveryDto?> SetIncorporationDateAsync(Guid deliveryId, DateTime incorporationDate, Guid adminId)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.Status != (int)DeliveryStatus.Hired)
            throw new InvalidOperationException("Solo se puede registrar la incorporacion de una entrega en estado Contratado.");

        delivery.IncorporationDate = incorporationDate;
        delivery.UpdatedAt = DateTime.UtcNow;
        delivery.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "SetDeliveryIncorporationDate", "PTCandidateDelivery", delivery.Id,
            $"{{\"incorporationDate\":\"{incorporationDate:yyyy-MM-dd}\"}}", null);

        return await GetDeliveryDtoAsync(deliveryId);
    }

    public async Task<DeliveryDto?> SetHiringDateAsync(Guid deliveryId, DateTime hiringDate, Guid adminId)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.Status != (int)DeliveryStatus.Hired)
            throw new InvalidOperationException("Solo se puede registrar la fecha de contratacion de una entrega en estado Contratado.");

        delivery.HiringDate = hiringDate;
        delivery.UpdatedAt = DateTime.UtcNow;
        delivery.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "SetDeliveryHiringDate", "PTCandidateDelivery", delivery.Id,
            $"{{\"hiringDate\":\"{hiringDate:yyyy-MM-dd}\"}}", null);

        return await GetDeliveryDtoAsync(deliveryId);
    }

    public async Task<DeliveryDto?> CloseProcessAsync(Guid deliveryId, CloseProcessDto dto, Guid adminId)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.Status != (int)DeliveryStatus.Hired)
            throw new InvalidOperationException("Solo se puede cerrar el proceso de una entrega en estado Contratado.");
        if (delivery.ProcessClosedAt != null)
            throw new InvalidOperationException("El proceso ya esta cerrado.");

        var warrantyDays = await _warranty.GetWarrantyDaysForVacancyAsync(delivery.PT_VacancyId);
        var (_, warrantyStatus) = WarrantyCalculator.Calculate(delivery.IncorporationDate, warrantyDays);
        if (warrantyStatus.HasValue && warrantyStatus.Value != WarrantyStatus.Vencida)
            throw new InvalidOperationException("La garantia todavia esta vigente, no se puede cerrar el proceso todavia.");

        var hasActiveReplacement = await _context.PT_WarrantyReplacements
            .AnyAsync(w => w.OriginalDeliveryId == deliveryId && w.Status == (int)WarrantyReplacementStatus.EnCurso && !w.IsDeleted);
        if (hasActiveReplacement)
            throw new InvalidOperationException("Hay una reposicion de garantia en curso sin resolver.");

        delivery.ProcessClosedAt = DateTime.UtcNow;
        delivery.ProcessClosedByUserId = adminId;
        delivery.ProcessClosureNotes = dto.Notes;
        delivery.UpdatedAt = DateTime.UtcNow;
        delivery.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "CloseDeliveryProcess", "PTCandidateDelivery", delivery.Id, null, null);

        return await GetDeliveryDtoAsync(deliveryId);
    }

    public async Task<DeliveryDto?> RecordFeedbackAsync(Guid deliveryId, RecordFeedbackDto dto, Guid adminId)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new InvalidOperationException("El rating debe estar entre 1 y 5.");

        var delivery = await _context.PT_CandidateDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        if (delivery == null) return null;
        if (delivery.ProcessClosedAt == null)
            throw new InvalidOperationException("Solo se puede registrar feedback una vez cerrado el proceso.");
        if (delivery.FeedbackRecordedAt != null)
            throw new InvalidOperationException("El feedback ya fue registrado.");

        delivery.FeedbackRating = dto.Rating;
        delivery.FeedbackComments = dto.Comments;
        delivery.FeedbackRecordedAt = DateTime.UtcNow;
        delivery.FeedbackRecordedByUserId = adminId;
        delivery.UpdatedAt = DateTime.UtcNow;
        delivery.UpdatedBy = adminId;
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "RecordDeliveryFeedback", "PTCandidateDelivery", delivery.Id,
            $"{{\"rating\":{dto.Rating}}}", null);

        return await GetDeliveryDtoAsync(deliveryId);
    }

    public async Task<List<DeliveryDto>> GetHiredDeliveriesByVacancyAsync(Guid vacancyId)
    {
        var deliveries = await _context.PT_CandidateDeliveries
            .Include(d => d.Candidate)
            .Include(d => d.Vacancy)
            .Include(d => d.Company)
            .Where(d => d.PT_VacancyId == vacancyId && d.Status == (int)DeliveryStatus.Hired && !d.IsDeleted)
            .OrderByDescending(d => d.DeliveredAt)
            .ToListAsync();

        var dtos = new List<DeliveryDto>();
        foreach (var d in deliveries)
            dtos.Add(await MapToDtoAsync(d));
        return dtos;
    }

    private async Task<DeliveryDto?> GetDeliveryDtoAsync(Guid deliveryId)
    {
        var delivery = await _context.PT_CandidateDeliveries
            .Include(d => d.Candidate)
            .Include(d => d.Vacancy)
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.Id == deliveryId && !d.IsDeleted);
        return delivery == null ? null : await MapToDtoAsync(delivery);
    }

    private async Task<DeliveryDto> MapToDtoAsync(PTCandidateDelivery d)
    {
        var verification = await _verificationStatus.GetVerificationStatusAsync(d.PT_CandidateId);
        var warrantyDays = await _warranty.GetWarrantyDaysForVacancyAsync(d.PT_VacancyId);
        var (warrantyEndsAt, warrantyStatus) = WarrantyCalculator.Calculate(d.IncorporationDate, warrantyDays);
        var hasActiveReplacement = await _context.PT_WarrantyReplacements
            .AnyAsync(w => w.OriginalDeliveryId == d.Id && w.Status == (int)WarrantyReplacementStatus.EnCurso && !w.IsDeleted);
        var canCloseProcess = d.Status == (int)DeliveryStatus.Hired
            && d.ProcessClosedAt == null
            && (!warrantyStatus.HasValue || warrantyStatus.Value == WarrantyStatus.Vencida)
            && !hasActiveReplacement;
        var canRecordFeedback = d.ProcessClosedAt != null && d.FeedbackRecordedAt == null;
        var processClosedByName = d.ProcessClosedByUserId.HasValue
            ? (await _context.SC_Users.Where(u => u.Id == d.ProcessClosedByUserId.Value).Select(u => u.Email).FirstOrDefaultAsync())
            : null;

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
            IsVerifiedTD = verification.IsVerifiedTD,
            HiringDate = d.HiringDate,
            IncorporationDate = d.IncorporationDate,
            WarrantyEndsAt = warrantyEndsAt,
            WarrantyStatus = (int?)warrantyStatus,
            HasActiveWarrantyReplacement = hasActiveReplacement,
            ProcessClosedAt = d.ProcessClosedAt,
            ProcessClosedByName = processClosedByName,
            ProcessClosureNotes = d.ProcessClosureNotes,
            CanCloseProcess = canCloseProcess,
            FeedbackRating = d.FeedbackRating,
            FeedbackComments = d.FeedbackComments,
            FeedbackRecordedAt = d.FeedbackRecordedAt,
            CanRecordFeedback = canRecordFeedback
        };
    }
}
