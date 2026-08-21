using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public RecruitmentService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<RecruitmentPipelineResultDto> GetPipelineAsync(
        int page, int pageSize, int? stage = null, Guid? assignedTo = null, string? search = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.SC_Users
            .Where(u => !u.IsDeleted && u.PrimaryRole == 0)
            .Join(_context.PT_CandidateRecruitments.Where(r => !r.IsDeleted),
                u => u.Id, r => r.SCUserId, (u, r) => new { u, r })
            .AsQueryable();

        if (stage.HasValue)
            query = query.Where(x => x.r.CurrentStage == stage.Value);

        if (assignedTo.HasValue)
            query = query.Where(x => x.r.AssignedToUserId == assignedTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(x =>
                x.u.Email.ToLower().Contains(lower) ||
                (x.u.Candidate != null && (
                    (x.u.Candidate.FirstName + " " + x.u.Candidate.LastName).ToLower().Contains(lower) ||
                    (x.u.Candidate.Title != null && x.u.Candidate.Title.ToLower().Contains(lower))
                ))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RecruitmentPipelineDto
            {
                Id = x.r.Id,
                UserId = x.u.Id,
                FullName = x.u.Candidate != null ? (x.u.Candidate.FirstName + " " + x.u.Candidate.LastName) : x.u.Email,
                Email = x.u.Email,
                Title = x.u.Candidate != null ? x.u.Candidate.Title : null,
                CurrentStage = x.r.CurrentStage,
                AssignedToName = x.r.AssignedToUser != null ? x.r.AssignedToUser.Email : null,
                AssignedToUserId = x.r.AssignedToUserId,
                AssignedAt = x.r.AssignedAt,
                StageEnteredAt = x.r.StageEnteredAt,
                CreatedAt = x.r.CreatedAt,
                Notes = x.r.Notes,
                InvestigationCompleted = x.r.InvestigationChecklist.Count(c => c.IsCompleted && !c.IsDeleted),
                InvestigationTotal = x.r.InvestigationChecklist.Count(c => !c.IsDeleted),
                Dismissal = x.r.Dismissal != null && !x.r.Dismissal.IsDeleted ? new DismissalInfoDto
                {
                    Reason = x.r.Dismissal.Reason,
                    Notes = x.r.Dismissal.Notes,
                    DismissedByName = x.r.Dismissal.DismissedByUser != null ? x.r.Dismissal.DismissedByUser.Email : "",
                    CreatedAt = x.r.Dismissal.CreatedAt
                } : null
            })
            .ToListAsync();

        var countByStage = await _context.PT_CandidateRecruitments
            .Where(r => !r.IsDeleted)
            .GroupBy(r => r.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Stage, x => x.Count);

        return new RecruitmentPipelineResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            CountByStage = countByStage
        };
    }

    public async Task<RecruitmentDetailDto?> GetDetailAsync(Guid id)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.User).ThenInclude(u => u!.Candidate)
            .Include(r => r.AssignedToUser)
            .Include(r => r.StageLogs!).ThenInclude(l => l.ChangedByUser)
            .Include(r => r.InvestigationChecklist!).ThenInclude(c => c.CompletedByUser)
            .Include(r => r.Dismissal!).ThenInclude(d => d.DismissedByUser)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (recruitment == null) return null;

        var user = recruitment.User;
        var candidate = user?.Candidate;

        return new RecruitmentDetailDto
        {
            Id = recruitment.Id,
            UserId = user!.Id,
            FullName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}" : user!.Email,
            Email = user!.Email,
            Title = candidate?.Title,
            Phone = candidate?.Phone,
            Country = candidate?.Country,
            City = candidate?.City,
            CurrentStage = recruitment.CurrentStage,
            AssignedToName = recruitment.AssignedToUser?.Email,
            AssignedToUserId = recruitment.AssignedToUserId,
            AssignedAt = recruitment.AssignedAt,
            StageEnteredAt = recruitment.StageEnteredAt,
            CreatedAt = recruitment.CreatedAt,
            Notes = recruitment.Notes,
            StageLogs = recruitment.StageLogs?.Where(l => !l.IsDeleted).Select(l => new StageLogDto
            {
                FromStage = l.FromStage,
                ToStage = l.ToStage,
                ChangedByName = l.ChangedByUser?.Email ?? "",
                CreatedAt = l.CreatedAt,
                Notes = l.Notes
            }).ToList() ?? new(),
            InvestigationChecklist = recruitment.InvestigationChecklist?.Where(c => !c.IsDeleted).Select(c => new InvestigationChecklistDto
            {
                Id = c.Id,
                Step = c.Step,
                IsCompleted = c.IsCompleted,
                CompletedAt = c.CompletedAt,
                CompletedByName = c.CompletedByUser?.Email,
                Notes = c.Notes,
                EvidenceUrl = c.EvidenceUrl
            }).ToList() ?? new(),
            Dismissal = recruitment.Dismissal != null && !recruitment.Dismissal.IsDeleted ? new DismissalInfoDto
            {
                Reason = recruitment.Dismissal.Reason,
                Notes = recruitment.Dismissal.Notes,
                DismissedByName = recruitment.Dismissal.DismissedByUser?.Email ?? "",
                CreatedAt = recruitment.Dismissal.CreatedAt
            } : null
        };
    }

    public async Task<RecruitmentPipelineDto> AssignCandidateAsync(AssignCandidateDto dto, Guid adminId, string? ipAddress)
    {
        var existing = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.SCUserId == dto.UserId && !r.IsDeleted);

        if (existing != null)
        {
            existing.AssignedToUserId = dto.AssignedToUserId;
            existing.AssignedByUserId = adminId;
            existing.AssignedAt = DateTime.UtcNow;
            existing.Notes = dto.Notes ?? existing.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = adminId;
        }
        else
        {
            existing = new PTCandidateRecruitment
            {
                SCUserId = dto.UserId,
                PT_VacancyId = dto.VacancyId,
                CurrentStage = 0,
                AssignedToUserId = dto.AssignedToUserId,
                AssignedByUserId = adminId,
                AssignedAt = DateTime.UtcNow,
                StageEnteredAt = DateTime.UtcNow,
                Notes = dto.Notes,
                CreatedBy = adminId
            };
            _context.PT_CandidateRecruitments.Add(existing);
        }

        await _context.SaveChangesAsync();

        var checklistSteps = new[] { 0, 1, 2, 3, 4 };
        foreach (var step in checklistSteps)
        {
            var exists = await _context.PT_InvestigationChecklists
                .AnyAsync(c => c.PT_CandidateRecruitmentId == existing.Id && c.Step == step && !c.IsDeleted);
            if (!exists)
            {
                _context.PT_InvestigationChecklists.Add(new PTInvestigationChecklist
                {
                    PT_CandidateRecruitmentId = existing.Id,
                    Step = step,
                    IsCompleted = false,
                    CreatedBy = adminId
                });
            }
        }
        await _context.SaveChangesAsync();

        await _auditLog.LogAsync(adminId, "AssignCandidate", "PT_CandidateRecruitments", existing.Id, null, ipAddress);

        var user = await _context.SC_Users.Include(u => u.Candidate).FirstOrDefaultAsync(u => u.Id == dto.UserId);
        var assignee = await _context.SC_Users.FirstOrDefaultAsync(u => u.Id == dto.AssignedToUserId);

        return new RecruitmentPipelineDto
        {
            Id = existing.Id,
            UserId = dto.UserId,
            FullName = user?.Candidate != null ? $"{user.Candidate.FirstName} {user.Candidate.LastName}" : user?.Email ?? "",
            Email = user?.Email ?? "",
            Title = user?.Candidate?.Title,
            CurrentStage = existing.CurrentStage,
            AssignedToName = assignee?.Email,
            AssignedToUserId = dto.AssignedToUserId,
            AssignedAt = existing.AssignedAt,
            StageEnteredAt = existing.StageEnteredAt,
            CreatedAt = existing.CreatedAt,
            Notes = existing.Notes
        };
    }

    public async Task<bool> MoveStageAsync(Guid recruitmentId, int toStage, string? notes, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments.FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return false;

        var fromStage = recruitment.CurrentStage;

        if (toStage == 1 && fromStage == 0)
        {
            var steps = new[] { 0, 1, 2, 3, 4 };
            foreach (var step in steps)
            {
                var exists = await _context.PT_InvestigationChecklists
                    .AnyAsync(c => c.PT_CandidateRecruitmentId == recruitmentId && c.Step == step && !c.IsDeleted);
                if (!exists)
                {
                    _context.PT_InvestigationChecklists.Add(new PTInvestigationChecklist
                    {
                        PT_CandidateRecruitmentId = recruitmentId,
                        Step = step,
                        IsCompleted = false,
                        CreatedBy = adminId
                    });
                }
            }
        }

        recruitment.CurrentStage = toStage;
        recruitment.StageEnteredAt = DateTime.UtcNow;
        recruitment.UpdatedAt = DateTime.UtcNow;
        recruitment.UpdatedBy = adminId;

        _context.PT_RecruitmentStageLogs.Add(new PTRecruitmentStageLog
        {
            PT_CandidateRecruitmentId = recruitmentId,
            FromStage = fromStage,
            ToStage = toStage,
            ChangedByUserId = adminId,
            Notes = notes,
            CreatedBy = adminId
        });

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "MoveStage", "PT_CandidateRecruitments", recruitmentId, null, ipAddress);
        return true;
    }

    public async Task<bool> ToggleInvestigationStepAsync(Guid recruitmentId, ToggleInvestigationStepDto dto, Guid adminId, string? ipAddress)
    {
        var item = await _context.PT_InvestigationChecklists
            .FirstOrDefaultAsync(c => c.PT_CandidateRecruitmentId == recruitmentId && c.Step == dto.Step && !c.IsDeleted);

        if (item == null)
        {
            item = new PTInvestigationChecklist
            {
                PT_CandidateRecruitmentId = recruitmentId,
                Step = dto.Step,
                IsCompleted = dto.IsCompleted,
                CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null,
                CompletedByUserId = dto.IsCompleted ? adminId : null,
                Notes = dto.Notes,
                EvidenceUrl = dto.EvidenceUrl,
                CreatedBy = adminId
            };
            _context.PT_InvestigationChecklists.Add(item);
        }
        else
        {
            item.IsCompleted = dto.IsCompleted;
            item.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;
            item.CompletedByUserId = dto.IsCompleted ? adminId : null;
            item.Notes = dto.Notes ?? item.Notes;
            item.EvidenceUrl = dto.EvidenceUrl ?? item.EvidenceUrl;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = adminId;
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "ToggleInvestigationStep", "PT_InvestigationChecklists", item.Id, null, ipAddress);
        return true;
    }

    public async Task<bool> DismissCandidateAsync(Guid recruitmentId, DismissCandidateDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments.FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return false;

        var fromStage = recruitment.CurrentStage;
        recruitment.CurrentStage = 5;
        recruitment.UpdatedAt = DateTime.UtcNow;
        recruitment.UpdatedBy = adminId;

        var existingDismissal = await _context.PT_RecruitmentDismissals
            .FirstOrDefaultAsync(d => d.PT_CandidateRecruitmentId == recruitmentId && !d.IsDeleted);

        if (existingDismissal != null)
        {
            existingDismissal.Reason = dto.Reason;
            existingDismissal.Notes = dto.Notes;
            existingDismissal.DismissedByUserId = adminId;
            existingDismissal.UpdatedAt = DateTime.UtcNow;
            existingDismissal.UpdatedBy = adminId;
        }
        else
        {
            _context.PT_RecruitmentDismissals.Add(new PTRecruitmentDismissal
            {
                PT_CandidateRecruitmentId = recruitmentId,
                Reason = dto.Reason,
                Notes = dto.Notes,
                DismissedByUserId = adminId,
                CreatedBy = adminId
            });
        }

        _context.PT_RecruitmentStageLogs.Add(new PTRecruitmentStageLog
        {
            PT_CandidateRecruitmentId = recruitmentId,
            FromStage = fromStage,
            ToStage = 5,
            ChangedByUserId = adminId,
            Notes = $"Dismissed: {dto.Notes}",
            CreatedBy = adminId
        });

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DismissCandidate", "PT_CandidateRecruitments", recruitmentId, null, ipAddress);
        return true;
    }

    public async Task<bool> UnassignAsync(Guid recruitmentId, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments.FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return false;

        recruitment.AssignedToUserId = null;
        recruitment.AssignedByUserId = null;
        recruitment.AssignedAt = null;
        recruitment.UpdatedAt = DateTime.UtcNow;
        recruitment.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UnassignCandidate", "PT_CandidateRecruitments", recruitmentId, null, ipAddress);
        return true;
    }
}
