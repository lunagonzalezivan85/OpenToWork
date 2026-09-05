using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class NegotiationService : INegotiationService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public NegotiationService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<NegotiationDto?> CreateAsync(CreateNegotiationDto dto, Guid staffId)
    {
        if (dto.CandidateIds.Count == 0) return null;

        var vacancyExists = await _context.PT_Vacancies.AnyAsync(v => v.Id == dto.VacancyId && !v.IsDeleted);
        if (!vacancyExists) return null;

        var validCandidateIds = await _context.PT_Candidates
            .Where(c => dto.CandidateIds.Contains(c.Id) && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();
        if (validCandidateIds.Count == 0) return null;

        // El shortlist (compatibilidad calculada, Fase 3) no exige que el candidato haya
        // aplicado antes — Trato Directo cura y presenta candidatos directamente. Se reutiliza
        // la PT_Application existente si ya aplico, o se crea una nueva (AdminCurated) si no.
        var existingApplications = await _context.PT_Applications
            .Where(a => a.PT_VacancyId == dto.VacancyId && validCandidateIds.Contains(a.PT_CandidateId) && !a.IsDeleted)
            .ToListAsync();

        var applicationIds = new List<Guid>();
        foreach (var candidateId in validCandidateIds)
        {
            var existing = existingApplications.FirstOrDefault(a => a.PT_CandidateId == candidateId);
            if (existing != null)
            {
                applicationIds.Add(existing.Id);
                continue;
            }

            var newApplication = new PTApplication
            {
                PT_CandidateId = candidateId,
                PT_VacancyId = dto.VacancyId,
                Status = (int)ApplicationStatus.Pending,
                ApplicationSource = (int)ApplicationSource.AdminCurated,
                CreatedBy = staffId
            };
            _context.PT_Applications.Add(newApplication);
            applicationIds.Add(newApplication.Id);
        }

        var negotiation = new PTNegotiation
        {
            PT_VacancyId = dto.VacancyId,
            Status = (int)NegotiationStatus.Presentada,
            AssignedStaffId = staffId,
            PresentedAt = DateTime.UtcNow,
            CreatedBy = staffId
        };

        foreach (var applicationId in applicationIds)
        {
            negotiation.Candidates.Add(new PTNegotiationCandidate
            {
                PT_ApplicationId = applicationId,
                CreatedBy = staffId
            });
        }

        _context.PT_Negotiations.Add(negotiation);
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "PresentNegotiation", "PT_Negotiations", negotiation.Id,
            $"{{\"vacancyId\":\"{dto.VacancyId}\",\"candidateCount\":{applicationIds.Count}}}", null);

        return await ToDtoAsync(negotiation.Id);
    }

    public async Task<NegotiationDto?> UpdateStatusAsync(Guid id, int status)
    {
        if (!Enum.IsDefined(typeof(NegotiationStatus), status)) return null;

        var negotiation = await _context.PT_Negotiations.FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        if (negotiation == null) return null;

        negotiation.Status = status;
        negotiation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await ToDtoAsync(id);
    }

    public async Task<NegotiationDto?> CloseAsync(Guid id, Guid winningApplicationId, Guid staffId, string? ipAddress)
    {
        var negotiation = await _context.PT_Negotiations
            .Include(n => n.Candidates)
            .Include(n => n.Vacancy)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        if (negotiation == null) return null;

        var candidateApplicationIds = negotiation.Candidates.Select(c => c.PT_ApplicationId).ToList();
        if (!candidateApplicationIds.Contains(winningApplicationId)) return null;

        var applications = await _context.PT_Applications
            .Where(a => candidateApplicationIds.Contains(a.Id))
            .ToListAsync();

        foreach (var application in applications)
        {
            application.Status = application.Id == winningApplicationId
                ? (int)ApplicationStatus.Accepted
                : (int)ApplicationStatus.Rejected;
            application.UpdatedAt = DateTime.UtcNow;
            application.UpdatedBy = staffId;
        }

        negotiation.Status = (int)NegotiationStatus.Cerrada;
        negotiation.ClosedAt = DateTime.UtcNow;
        negotiation.WinningApplicationId = winningApplicationId;
        negotiation.UpdatedAt = DateTime.UtcNow;
        negotiation.UpdatedBy = staffId;

        negotiation.Vacancy.Status = (int)VacancyStatus.Closed;
        negotiation.Vacancy.ClosedAt = DateTime.UtcNow;
        negotiation.Vacancy.UpdatedAt = DateTime.UtcNow;
        negotiation.Vacancy.UpdatedBy = staffId;

        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(staffId, "CloseNegotiation", "PT_Negotiations", negotiation.Id,
            $"{{\"winningApplicationId\":\"{winningApplicationId}\",\"vacancyId\":\"{negotiation.PT_VacancyId}\"}}", ipAddress);

        return await ToDtoAsync(id);
    }

    public async Task<List<NegotiationDto>> GetByVacancyAsync(Guid vacancyId)
    {
        var ids = await _context.PT_Negotiations
            .Where(n => n.PT_VacancyId == vacancyId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => n.Id)
            .ToListAsync();

        var result = new List<NegotiationDto>();
        foreach (var id in ids)
        {
            var dto = await ToDtoAsync(id);
            if (dto != null) result.Add(dto);
        }
        return result;
    }

    private async Task<NegotiationDto?> ToDtoAsync(Guid negotiationId)
    {
        var negotiation = await _context.PT_Negotiations
            .Include(n => n.Vacancy)
            .Include(n => n.Candidates).ThenInclude(c => c.Application).ThenInclude(a => a.Candidate)
            .FirstOrDefaultAsync(n => n.Id == negotiationId);
        if (negotiation == null) return null;

        return new NegotiationDto
        {
            Id = negotiation.Id,
            VacancyId = negotiation.PT_VacancyId,
            VacancyTitle = negotiation.Vacancy?.Title,
            Status = negotiation.Status,
            AssignedStaffId = negotiation.AssignedStaffId,
            PresentedAt = negotiation.PresentedAt,
            ClosedAt = negotiation.ClosedAt,
            WinningApplicationId = negotiation.WinningApplicationId,
            Notes = negotiation.Notes,
            Candidates = negotiation.Candidates
                .Where(c => !c.IsDeleted)
                .Select(c => new NegotiationCandidateDto
                {
                    ApplicationId = c.PT_ApplicationId,
                    CandidateId = c.Application.PT_CandidateId,
                    CandidateName = c.Application.Candidate != null
                        ? $"{c.Application.Candidate.FirstName} {c.Application.Candidate.LastName}".Trim()
                        : null,
                    ApplicationStatus = c.Application.Status
                })
                .ToList()
        };
    }
}
