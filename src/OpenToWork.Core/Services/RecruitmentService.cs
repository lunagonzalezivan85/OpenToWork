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
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Experiences)
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Certifications)
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Educations)
            .Include(r => r.AssignedToUser)
            .Include(r => r.Vacancy)
            .Include(r => r.StageLogs!).ThenInclude(l => l.ChangedByUser)
            .Include(r => r.InvestigationChecklist!).ThenInclude(c => c.CompletedByUser)
            .Include(r => r.InvestigationChecklist!).ThenInclude(c => c.ReferenceChecks)
            .Include(r => r.TechnicalEvaluations!).ThenInclude(t => t.EvaluatedByUser)
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
            LinkedInUrl = candidate?.LinkedInUrl,
            PortfolioUrl = candidate?.PortfolioUrl,
            Nationality = candidate?.Nationality,
            HasPassport = candidate?.HasPassport,
            PassportNumber = candidate?.PassportNumber,
            WorkAuthorization = candidate?.WorkAuthorization,
            WorkAuthorizations = candidate?.WorkAuthorizations,
            HasTransport = candidate?.HasTransport,
            VacancyId = recruitment.PT_VacancyId,
            VacancyTitle = recruitment.Vacancy != null ? recruitment.Vacancy.Title : null,
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
            WorkExperiences = candidate?.Experiences?.Where(e => !e.IsDeleted).Select(e => new CandidateExperienceDto
            {
                Id = e.Id,
                CandidateId = e.PT_CandidateId,
                CompanyName = e.CompanyName,
                JobTitle = e.JobTitle,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrentJob = e.IsCurrentJob,
                Location = e.Location
            }).ToList() ?? new(),
            Certifications = candidate?.Certifications?.Where(c => !c.IsDeleted).Select(c => new CandidateCertificationDto
            {
                Id = c.Id,
                CandidateId = c.PT_CandidateId,
                Name = c.Name,
                Issuer = c.Issuer,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                CredentialId = c.CredentialId,
                CredentialUrl = c.CredentialUrl
            }).ToList() ?? new(),
            Educations = candidate?.Educations?.Where(e => !e.IsDeleted).Select(e => new CandidateEducationDto
            {
                Id = e.Id,
                CandidateId = e.PT_CandidateId,
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsInProgress = e.IsInProgress
            }).ToList() ?? new(),
            TechnicalEvaluations = recruitment.TechnicalEvaluations?.Where(t => !t.IsDeleted && t.Type == 0).OrderByDescending(t => t.EvaluatedAt ?? t.CreatedAt).Select(t => new TechnicalEvaluationDto
            {
                Id = t.Id,
                EvaluationName = t.EvaluationName,
                Description = t.Description,
                Score = t.Score,
                EvidenceUrl = t.EvidenceUrl,
                Notes = t.Notes,
                EvaluatedAt = t.EvaluatedAt,
                EvaluatedByName = t.EvaluatedByUser?.Email,
                Type = t.Type,
                Recommendation = t.Recommendation
            }).ToList() ?? new(),
            CulturalInterviews = recruitment.TechnicalEvaluations?.Where(t => !t.IsDeleted && t.Type == 1).OrderByDescending(t => t.EvaluatedAt ?? t.CreatedAt).Select(t => new TechnicalEvaluationDto
            {
                Id = t.Id,
                EvaluationName = t.EvaluationName,
                Description = t.Description,
                Score = t.Score,
                EvidenceUrl = t.EvidenceUrl,
                Notes = t.Notes,
                EvaluatedAt = t.EvaluatedAt,
                EvaluatedByName = t.EvaluatedByUser?.Email,
                Type = t.Type,
                Recommendation = t.Recommendation
            }).ToList() ?? new(),
            InvestigationChecklist = recruitment.InvestigationChecklist?.Where(c => !c.IsDeleted).OrderBy(c => c.Step).Select(c => new InvestigationChecklistDto
            {
                Id = c.Id,
                Step = c.Step,
                Label = c.Label,
                IsCustom = c.IsCustom,
                IsCompleted = c.IsCompleted,
                StartedAt = c.StartedAt,
                CompletedAt = c.CompletedAt,
                CompletedByName = c.CompletedByUser?.Email,
                Notes = c.Notes,
                EvidenceUrl = c.EvidenceUrl,
                ReferenceChecks = c.ReferenceChecks != null ? c.ReferenceChecks.Where(r => !r.IsDeleted).Select(r => new ReferenceCheckDto
                {
                    Id = r.Id,
                    CompanyName = r.CompanyName,
                    ContactName = r.ContactName,
                    ContactPhone = r.ContactPhone,
                    ContactEmail = r.ContactEmail,
                    Status = r.Status,
                    CalledAt = r.CalledAt,
                    Notes = r.Notes
                }).ToList() : new()
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

    public async Task<RecruitmentDetailDto?> GetByUserIdAsync(Guid userId)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.User).ThenInclude(u => u!.Candidate)
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Experiences)
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Certifications)
            .Include(r => r.User).ThenInclude(u => u!.Candidate!).ThenInclude(c => c.Educations)
            .Include(r => r.AssignedToUser)
            .Include(r => r.Vacancy)
            .Include(r => r.StageLogs!).ThenInclude(l => l.ChangedByUser)
            .Include(r => r.InvestigationChecklist!).ThenInclude(c => c.CompletedByUser)
            .Include(r => r.InvestigationChecklist!).ThenInclude(c => c.ReferenceChecks)
            .Include(r => r.TechnicalEvaluations!).ThenInclude(t => t.EvaluatedByUser)
            .Include(r => r.Dismissal!).ThenInclude(d => d.DismissedByUser)
            .FirstOrDefaultAsync(r => r.SCUserId == userId && !r.IsDeleted);

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
            LinkedInUrl = candidate?.LinkedInUrl,
            PortfolioUrl = candidate?.PortfolioUrl,
            Nationality = candidate?.Nationality,
            HasPassport = candidate?.HasPassport,
            PassportNumber = candidate?.PassportNumber,
            WorkAuthorization = candidate?.WorkAuthorization,
            WorkAuthorizations = candidate?.WorkAuthorizations,
            HasTransport = candidate?.HasTransport,
            VacancyId = recruitment.PT_VacancyId,
            VacancyTitle = recruitment.Vacancy != null ? recruitment.Vacancy.Title : null,
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
            WorkExperiences = candidate?.Experiences?.Where(e => !e.IsDeleted).Select(e => new CandidateExperienceDto
            {
                Id = e.Id,
                CandidateId = e.PT_CandidateId,
                CompanyName = e.CompanyName,
                JobTitle = e.JobTitle,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrentJob = e.IsCurrentJob,
                Location = e.Location
            }).ToList() ?? new(),
            Certifications = candidate?.Certifications?.Where(c => !c.IsDeleted).Select(c => new CandidateCertificationDto
            {
                Id = c.Id,
                CandidateId = c.PT_CandidateId,
                Name = c.Name,
                Issuer = c.Issuer,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                CredentialId = c.CredentialId,
                CredentialUrl = c.CredentialUrl
            }).ToList() ?? new(),
            Educations = candidate?.Educations?.Where(e => !e.IsDeleted).Select(e => new CandidateEducationDto
            {
                Id = e.Id,
                CandidateId = e.PT_CandidateId,
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsInProgress = e.IsInProgress
            }).ToList() ?? new(),
            TechnicalEvaluations = recruitment.TechnicalEvaluations?.Where(t => !t.IsDeleted && t.Type == 0).OrderByDescending(t => t.EvaluatedAt ?? t.CreatedAt).Select(t => new TechnicalEvaluationDto
            {
                Id = t.Id,
                EvaluationName = t.EvaluationName,
                Description = t.Description,
                Score = t.Score,
                EvidenceUrl = t.EvidenceUrl,
                Notes = t.Notes,
                EvaluatedAt = t.EvaluatedAt,
                EvaluatedByName = t.EvaluatedByUser?.Email,
                Type = t.Type,
                Recommendation = t.Recommendation
            }).ToList() ?? new(),
            CulturalInterviews = recruitment.TechnicalEvaluations?.Where(t => !t.IsDeleted && t.Type == 1).OrderByDescending(t => t.EvaluatedAt ?? t.CreatedAt).Select(t => new TechnicalEvaluationDto
            {
                Id = t.Id,
                EvaluationName = t.EvaluationName,
                Description = t.Description,
                Score = t.Score,
                EvidenceUrl = t.EvidenceUrl,
                Notes = t.Notes,
                EvaluatedAt = t.EvaluatedAt,
                EvaluatedByName = t.EvaluatedByUser?.Email,
                Type = t.Type,
                Recommendation = t.Recommendation
            }).ToList() ?? new(),
            InvestigationChecklist = recruitment.InvestigationChecklist?.Where(c => !c.IsDeleted).OrderBy(c => c.Step).Select(c => new InvestigationChecklistDto
            {
                Id = c.Id,
                Step = c.Step,
                Label = c.Label,
                IsCustom = c.IsCustom,
                IsCompleted = c.IsCompleted,
                StartedAt = c.StartedAt,
                CompletedAt = c.CompletedAt,
                CompletedByName = c.CompletedByUser?.Email,
                Notes = c.Notes,
                EvidenceUrl = c.EvidenceUrl,
                ReferenceChecks = c.ReferenceChecks != null ? c.ReferenceChecks.Where(r => !r.IsDeleted).Select(r => new ReferenceCheckDto
                {
                    Id = r.Id,
                    CompanyName = r.CompanyName,
                    ContactName = r.ContactName,
                    ContactPhone = r.ContactPhone,
                    ContactEmail = r.ContactEmail,
                    Status = r.Status,
                    CalledAt = r.CalledAt,
                    Notes = r.Notes
                }).ToList() : new()
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

        var defaultSteps = new[]
        {
            new { Step = 0, Label = "Llamar al candidato" },
            new { Step = 1, Label = "Llamar a las referencias (mínimo 3)" },
            new { Step = 2, Label = "Validar LinkedIn" },
            new { Step = 3, Label = "Validar portafolio" },
            new { Step = 4, Label = "Validar certificaciones" }
        };
        foreach (var s in defaultSteps)
        {
            var exists = await _context.PT_InvestigationChecklists
                .AnyAsync(c => c.PT_CandidateRecruitmentId == existing.Id && c.Step == s.Step && !c.IsDeleted);
            if (!exists)
            {
                _context.PT_InvestigationChecklists.Add(new PTInvestigationChecklist
                {
                    PT_CandidateRecruitmentId = existing.Id,
                    Step = s.Step,
                    Label = s.Label,
                    IsCustom = false,
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
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.Preferences)
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return false;

        if (toStage > 0 && (recruitment.Preferences == null || !recruitment.Preferences.IsCompleted))
            return false;

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
            await _context.SaveChangesAsync();

            // Auto-generate references from candidate work experiences
            var referencesChecklist = await _context.PT_InvestigationChecklists
                .FirstOrDefaultAsync(c => c.PT_CandidateRecruitmentId == recruitmentId && c.Step == 1 && !c.IsDeleted);

            if (referencesChecklist != null)
            {
                var candidate = await _context.SC_Users
                    .Include(u => u.Candidate!).ThenInclude(c => c.Experiences)
                    .FirstOrDefaultAsync(u => u.Id == recruitment.SCUserId);

                var experiences = candidate?.Candidate?.Experiences?
                    .Where(e => !e.IsDeleted)
                    .GroupBy(e => e.CompanyName)
                    .Select(g => g.First())
                    .ToList();

                if (experiences != null && experiences.Count > 0)
                {
                    var existingRefs = await _context.PT_ReferenceChecks
                        .Where(r => r.PT_InvestigationChecklistId == referencesChecklist.Id && !r.IsDeleted)
                        .Select(r => r.CompanyName)
                        .ToListAsync();

                    foreach (var exp in experiences)
                    {
                        if (!existingRefs.Contains(exp.CompanyName))
                        {
                            _context.PT_ReferenceChecks.Add(new PTReferenceCheck
                            {
                                PT_InvestigationChecklistId = referencesChecklist.Id,
                                CompanyName = exp.CompanyName,
                                ContactName = null,
                                ContactPhone = null,
                                ContactEmail = null,
                                Status = 0,
                                CreatedBy = adminId
                            });
                        }
                    }
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
                StartedAt = dto.IsCompleted ? DateTime.UtcNow : null,
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
            if (dto.IsCompleted && !item.StartedAt.HasValue)
                item.StartedAt = DateTime.UtcNow;

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

    public async Task<bool> StartInvestigationStepAsync(Guid recruitmentId, int step, Guid adminId, string? ipAddress)
    {
        var item = await _context.PT_InvestigationChecklists
            .FirstOrDefaultAsync(c => c.PT_CandidateRecruitmentId == recruitmentId && c.Step == step && !c.IsDeleted);

        if (item == null) return false;
        if (item.StartedAt.HasValue) return true;

        item.StartedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "StartInvestigationStep", "PT_InvestigationChecklists", item.Id, null, ipAddress);
        return true;
    }

    public async Task<ReferenceCheckDto?> AddReferenceAsync(Guid checklistId, AddReferenceDto dto, Guid adminId, string? ipAddress)
    {
        var checklist = await _context.PT_InvestigationChecklists
            .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDeleted);
        if (checklist == null) return null;

        if (!checklist.StartedAt.HasValue)
        {
            checklist.StartedAt = DateTime.UtcNow;
        }

        var reference = new PTReferenceCheck
        {
            PT_InvestigationChecklistId = checklistId,
            CompanyName = dto.CompanyName,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            Status = 0,
            CreatedBy = adminId
        };
        _context.PT_ReferenceChecks.Add(reference);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "AddReference", "PT_ReferenceChecks", reference.Id, null, ipAddress);

        return new ReferenceCheckDto
        {
            Id = reference.Id,
            CompanyName = reference.CompanyName,
            ContactName = reference.ContactName,
            ContactPhone = reference.ContactPhone,
            ContactEmail = reference.ContactEmail,
            Status = 0
        };
    }

    public async Task<bool> UpdateReferenceStatusAsync(Guid referenceId, UpdateReferenceStatusDto dto, Guid adminId, string? ipAddress)
    {
        var reference = await _context.PT_ReferenceChecks
            .FirstOrDefaultAsync(r => r.Id == referenceId && !r.IsDeleted);
        if (reference == null) return false;

        reference.Status = dto.Status;
        reference.Notes = dto.Notes ?? reference.Notes;
        if (dto.Status >= 1 && !reference.CalledAt.HasValue)
            reference.CalledAt = DateTime.UtcNow;
        reference.UpdatedAt = DateTime.UtcNow;
        reference.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateReferenceStatus", "PT_ReferenceChecks", referenceId, null, ipAddress);
        return true;
    }

    public async Task<bool> DeleteReferenceAsync(Guid referenceId, Guid adminId, string? ipAddress)
    {
        var reference = await _context.PT_ReferenceChecks
            .FirstOrDefaultAsync(r => r.Id == referenceId && !r.IsDeleted);
        if (reference == null) return false;

        reference.IsDeleted = true;
        reference.DeletedAt = DateTime.UtcNow;
        reference.DeletedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteReference", "PT_ReferenceChecks", referenceId, null, ipAddress);
        return true;
    }

    public async Task<InvestigationChecklistDto?> AddCustomValidationAsync(Guid recruitmentId, AddCustomValidationDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return null;

        var maxStep = await _context.PT_InvestigationChecklists
            .Where(c => c.PT_CandidateRecruitmentId == recruitmentId && !c.IsDeleted)
            .MaxAsync(c => (int?)c.Step) ?? 4;

        var item = new PTInvestigationChecklist
        {
            PT_CandidateRecruitmentId = recruitmentId,
            Step = maxStep + 1,
            Label = dto.Label,
            IsCustom = true,
            IsCompleted = false,
            CreatedBy = adminId
        };
        _context.PT_InvestigationChecklists.Add(item);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "AddCustomValidation", "PT_InvestigationChecklists", item.Id, null, ipAddress);

        return new InvestigationChecklistDto
        {
            Id = item.Id,
            Step = item.Step,
            Label = item.Label,
            IsCustom = item.IsCustom,
            IsCompleted = false
        };
    }

    public async Task<bool> DeleteCustomValidationAsync(Guid checklistId, Guid adminId, string? ipAddress)
    {
        var item = await _context.PT_InvestigationChecklists
            .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDeleted && c.IsCustom);
        if (item == null) return false;

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = adminId;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteCustomValidation", "PT_InvestigationChecklists", item.Id, null, ipAddress);
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

    public async Task<bool> RestoreCandidateAsync(Guid recruitmentId, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null || recruitment.CurrentStage != 5) return false;

        var lastLog = await _context.PT_RecruitmentStageLogs
            .Where(l => l.PT_CandidateRecruitmentId == recruitmentId && l.ToStage == 5 && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        var restoreStage = lastLog?.FromStage ?? 0;
        recruitment.CurrentStage = restoreStage;
        recruitment.UpdatedAt = DateTime.UtcNow;
        recruitment.UpdatedBy = adminId;

        var dismissal = await _context.PT_RecruitmentDismissals
            .FirstOrDefaultAsync(d => d.PT_CandidateRecruitmentId == recruitmentId && !d.IsDeleted);
        if (dismissal != null)
        {
            dismissal.IsDeleted = true;
            dismissal.UpdatedAt = DateTime.UtcNow;
            dismissal.UpdatedBy = adminId;
        }

        _context.PT_RecruitmentStageLogs.Add(new PTRecruitmentStageLog
        {
            PT_CandidateRecruitmentId = recruitmentId,
            FromStage = 5,
            ToStage = restoreStage,
            ChangedByUserId = adminId,
            Notes = "Restored from dismissal",
            CreatedBy = adminId
        });

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "RestoreCandidate", "PT_CandidateRecruitments", recruitmentId, null, ipAddress);
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

    public async Task<bool> UpdateCandidatePhoneAsync(Guid recruitmentId, string? phone, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.User).ThenInclude(u => u!.Candidate)
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);

        if (recruitment?.User?.Candidate == null) return false;

        recruitment.User.Candidate.Phone = phone;
        recruitment.User.Candidate.UpdatedAt = DateTime.UtcNow;
        recruitment.User.Candidate.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateCandidatePhone", "PT_Candidates", recruitment.User.Candidate.Id, null, ipAddress);
        return true;
    }

    public async Task<bool> UpdateChecklistNotesAsync(Guid checklistId, string? notes, Guid adminId, string? ipAddress)
    {
        var item = await _context.PT_InvestigationChecklists
            .FirstOrDefaultAsync(c => c.Id == checklistId && !c.IsDeleted);
        if (item == null) return false;

        item.Notes = notes;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateChecklistNotes", "PT_InvestigationChecklists", checklistId, null, ipAddress);
        return true;
    }

    public async Task<TechnicalEvaluationDto?> AddTechnicalEvaluationAsync(Guid recruitmentId, AddTechnicalEvaluationDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments.FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return null;

        var evaluation = new PTTechnicalEvaluation
        {
            PT_CandidateRecruitmentId = recruitmentId,
            EvaluationName = dto.EvaluationName,
            Description = dto.Description,
            Score = dto.Score,
            EvidenceUrl = dto.EvidenceUrl,
            Notes = dto.Notes,
            EvaluatedAt = DateTime.UtcNow,
            EvaluatedByUserId = adminId,
            Type = dto.Type,
            Recommendation = dto.Recommendation,
            CreatedBy = adminId
        };

        _context.PT_TechnicalEvaluations.Add(evaluation);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "AddTechnicalEvaluation", "PT_TechnicalEvaluations", evaluation.Id, null, ipAddress);

        return new TechnicalEvaluationDto
        {
            Id = evaluation.Id,
            EvaluationName = evaluation.EvaluationName,
            Description = evaluation.Description,
            Score = evaluation.Score,
            EvidenceUrl = evaluation.EvidenceUrl,
            Notes = evaluation.Notes,
            EvaluatedAt = evaluation.EvaluatedAt,
            EvaluatedByName = ""
        };
    }

    public async Task<bool> UpdateTechnicalEvaluationAsync(Guid evaluationId, UpdateTechnicalEvaluationDto dto, Guid adminId, string? ipAddress)
    {
        var evaluation = await _context.PT_TechnicalEvaluations.FirstOrDefaultAsync(t => t.Id == evaluationId && !t.IsDeleted);
        if (evaluation == null) return false;

        if (dto.EvaluationName != null) evaluation.EvaluationName = dto.EvaluationName;
        if (dto.Description != null) evaluation.Description = dto.Description;
        if (dto.Score.HasValue) evaluation.Score = dto.Score.Value;
        if (dto.EvidenceUrl != null) evaluation.EvidenceUrl = dto.EvidenceUrl;
        if (dto.Notes != null) evaluation.Notes = dto.Notes;
        if (dto.Recommendation.HasValue) evaluation.Recommendation = dto.Recommendation.Value;
        evaluation.UpdatedAt = DateTime.UtcNow;
        evaluation.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateTechnicalEvaluation", "PT_TechnicalEvaluations", evaluationId, null, ipAddress);
        return true;
    }

    public async Task<bool> DeleteTechnicalEvaluationAsync(Guid evaluationId, Guid adminId, string? ipAddress)
    {
        var evaluation = await _context.PT_TechnicalEvaluations.FirstOrDefaultAsync(t => t.Id == evaluationId && !t.IsDeleted);
        if (evaluation == null) return false;

        evaluation.IsDeleted = true;
        evaluation.UpdatedAt = DateTime.UtcNow;
        evaluation.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteTechnicalEvaluation", "PT_TechnicalEvaluations", evaluationId, null, ipAddress);
        return true;
    }

    public async Task<TechnicalEvaluationDto?> GetCulturalInterviewAsync(Guid recruitmentId)
    {
        var evaluation = await _context.PT_TechnicalEvaluations
            .Include(t => t.EvaluatedByUser)
            .Where(t => t.PT_CandidateRecruitmentId == recruitmentId && t.Type == 1 && !t.IsDeleted)
            .OrderByDescending(t => t.EvaluatedAt ?? t.CreatedAt)
            .FirstOrDefaultAsync();

        if (evaluation == null) return null;

        return new TechnicalEvaluationDto
        {
            Id = evaluation.Id,
            EvaluationName = evaluation.EvaluationName,
            Description = evaluation.Description,
            Score = evaluation.Score,
            EvidenceUrl = evaluation.EvidenceUrl,
            Notes = evaluation.Notes,
            EvaluatedAt = evaluation.EvaluatedAt,
            EvaluatedByName = evaluation.EvaluatedByUser?.Email,
            Type = evaluation.Type,
            Recommendation = evaluation.Recommendation
        };
    }

    public async Task<CandidateRecruitmentPreferencesDto?> GetPreferencesAsync(Guid recruitmentId)
    {
        var prefs = await _context.PT_CandidateRecruitmentPreferences
            .FirstOrDefaultAsync(p => p.PT_CandidateRecruitmentId == recruitmentId && !p.IsDeleted);

        if (prefs == null) return null;

        return new CandidateRecruitmentPreferencesDto
        {
            Id = prefs.Id,
            RecruitmentId = prefs.PT_CandidateRecruitmentId,
            PreferredWorkShift = prefs.PreferredWorkShift,
            AcceptedContractType = prefs.AcceptedContractType,
            AvailabilityToJoin = prefs.AvailabilityToJoin,
            ExpectedSalary = prefs.ExpectedSalary,
            AvailableDays = prefs.AvailableDays,
            AvailableWeekends = prefs.AvailableWeekends,
            AvailableHolidays = prefs.AvailableHolidays,
            AvailableSchedule = prefs.AvailableSchedule,
            IsCompleted = prefs.IsCompleted,
            CompletedAt = prefs.CompletedAt
        };
    }

    public async Task<CandidateRecruitmentPreferencesDto?> SavePreferencesAsync(
        Guid recruitmentId, UpdateRecruitmentPreferencesDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return null;

        var prefs = await _context.PT_CandidateRecruitmentPreferences
            .FirstOrDefaultAsync(p => p.PT_CandidateRecruitmentId == recruitmentId && !p.IsDeleted);

        var isCompleted = dto.PreferredWorkShift.HasValue
            && dto.AcceptedContractType.HasValue
            && dto.AvailabilityToJoin.HasValue
            && dto.ExpectedSalary.HasValue
            && !string.IsNullOrWhiteSpace(dto.AvailableDays)
            && dto.AvailableWeekends.HasValue
            && dto.AvailableHolidays.HasValue
            && !string.IsNullOrWhiteSpace(dto.AvailableSchedule);

        if (prefs == null)
        {
            prefs = new PTCandidateRecruitmentPreferences
            {
                PT_CandidateRecruitmentId = recruitmentId,
                PreferredWorkShift = dto.PreferredWorkShift,
                AcceptedContractType = dto.AcceptedContractType,
                AvailabilityToJoin = dto.AvailabilityToJoin,
                ExpectedSalary = dto.ExpectedSalary,
                AvailableDays = dto.AvailableDays,
                AvailableWeekends = dto.AvailableWeekends,
                AvailableHolidays = dto.AvailableHolidays,
                AvailableSchedule = dto.AvailableSchedule,
                IsCompleted = isCompleted,
                CompletedAt = isCompleted ? DateTime.UtcNow : null,
                CreatedBy = adminId
            };
            _context.PT_CandidateRecruitmentPreferences.Add(prefs);
        }
        else
        {
            prefs.PreferredWorkShift = dto.PreferredWorkShift;
            prefs.AcceptedContractType = dto.AcceptedContractType;
            prefs.AvailabilityToJoin = dto.AvailabilityToJoin;
            prefs.ExpectedSalary = dto.ExpectedSalary;
            prefs.AvailableDays = dto.AvailableDays;
            prefs.AvailableWeekends = dto.AvailableWeekends;
            prefs.AvailableHolidays = dto.AvailableHolidays;
            prefs.AvailableSchedule = dto.AvailableSchedule;
            prefs.IsCompleted = isCompleted;
            prefs.CompletedAt = isCompleted ? DateTime.UtcNow : prefs.CompletedAt;
            prefs.UpdatedAt = DateTime.UtcNow;
            prefs.UpdatedBy = adminId;
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "SaveRecruitmentPreferences", "PT_CandidateRecruitmentPreferences", prefs.Id, null, ipAddress);

        return new CandidateRecruitmentPreferencesDto
        {
            Id = prefs.Id,
            RecruitmentId = prefs.PT_CandidateRecruitmentId,
            PreferredWorkShift = prefs.PreferredWorkShift,
            AcceptedContractType = prefs.AcceptedContractType,
            AvailabilityToJoin = prefs.AvailabilityToJoin,
            ExpectedSalary = prefs.ExpectedSalary,
            AvailableDays = prefs.AvailableDays,
            AvailableWeekends = prefs.AvailableWeekends,
            AvailableHolidays = prefs.AvailableHolidays,
            AvailableSchedule = prefs.AvailableSchedule,
            IsCompleted = prefs.IsCompleted,
            CompletedAt = prefs.CompletedAt
        };
    }

    public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync()
    {
        return await _context.SY_DocumentTypes
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.SortOrder)
            .Select(d => new DocumentTypeDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Category = d.Category,
                IsRequired = d.IsRequired,
                SortOrder = d.SortOrder
            })
            .ToListAsync();
    }

    public async Task<List<RecruitmentDocumentDto>> GetDocumentsAsync(Guid recruitmentId)
    {
        return await _context.PT_RecruitmentDocuments
            .Include(r => r.DocumentType)
            .Include(r => r.VerifiedByUser)
            .Where(r => r.PT_CandidateRecruitmentId == recruitmentId && !r.IsDeleted)
            .OrderBy(r => r.DocumentType.SortOrder)
            .Select(r => new RecruitmentDocumentDto
            {
                Id = r.Id,
                RecruitmentId = r.PT_CandidateRecruitmentId,
                DocumentTypeId = r.SY_DocumentTypeId,
                DocumentTypeName = r.DocumentType.Name,
                DocumentTypeCategory = r.DocumentType.Category,
                Status = r.Status,
                FileUrl = r.FileUrl,
                FileName = r.FileName,
                SubmittedAt = r.SubmittedAt,
                VerifiedByName = r.VerifiedByUser != null ? r.VerifiedByUser.Email : null,
                VerifiedAt = r.VerifiedAt,
                Notes = r.Notes,
                ExpiresAt = r.ExpiresAt
            })
            .ToListAsync();
    }

    public async Task<RecruitmentDocumentDto?> RequestDocumentAsync(
        Guid recruitmentId, RequestDocumentDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return null;

        var existing = await _context.PT_RecruitmentDocuments
            .FirstOrDefaultAsync(r => r.PT_CandidateRecruitmentId == recruitmentId
                && r.SY_DocumentTypeId == dto.DocumentTypeId && !r.IsDeleted);
        if (existing != null) return null;

        var doc = new PTRecruitmentDocument
        {
            PT_CandidateRecruitmentId = recruitmentId,
            SY_DocumentTypeId = dto.DocumentTypeId,
            Status = 0,
            Notes = dto.Notes,
            CreatedBy = adminId
        };
        _context.PT_RecruitmentDocuments.Add(doc);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "RequestDocument", "PT_RecruitmentDocuments", doc.Id, null, ipAddress);

        var docType = await _context.SY_DocumentTypes.FirstOrDefaultAsync(d => d.Id == dto.DocumentTypeId);
        return new RecruitmentDocumentDto
        {
            Id = doc.Id,
            RecruitmentId = recruitmentId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentTypeName = docType?.Name ?? "",
            DocumentTypeCategory = docType?.Category,
            Status = 0,
            Notes = doc.Notes
        };
    }

    public async Task<bool> UpdateDocumentStatusAsync(
        Guid documentId, UpdateDocumentStatusDto dto, Guid adminId, string? ipAddress)
    {
        var doc = await _context.PT_RecruitmentDocuments
            .FirstOrDefaultAsync(r => r.Id == documentId && !r.IsDeleted);
        if (doc == null) return false;

        doc.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.FileUrl))
        {
            doc.FileUrl = dto.FileUrl;
            doc.FileName = dto.FileName;
            doc.SubmittedAt = DateTime.UtcNow;
        }
        if (dto.Status == 2)
        {
            doc.VerifiedByUserId = adminId;
            doc.VerifiedAt = DateTime.UtcNow;
        }
        doc.Notes = dto.Notes ?? doc.Notes;
        doc.ExpiresAt = dto.ExpiresAt ?? doc.ExpiresAt;
        doc.UpdatedAt = DateTime.UtcNow;
        doc.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateDocumentStatus", "PT_RecruitmentDocuments", documentId, null, ipAddress);
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid adminId, string? ipAddress)
    {
        var doc = await _context.PT_RecruitmentDocuments
            .FirstOrDefaultAsync(r => r.Id == documentId && !r.IsDeleted);
        if (doc == null) return false;

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeleteDocument", "PT_RecruitmentDocuments", documentId, null, ipAddress);
        return true;
    }

    public async Task<bool> UpdateMigrationInfoAsync(Guid recruitmentId, UpdateMigrationInfoDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .Include(r => r.User)
            .ThenInclude(u => u!.Candidate)
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);

        if (recruitment?.User?.Candidate == null) return false;

        var candidate = recruitment.User.Candidate;
        if (dto.Nationality != null) candidate.Nationality = string.IsNullOrWhiteSpace(dto.Nationality) ? null : dto.Nationality;
        if (dto.HasPassport.HasValue) candidate.HasPassport = dto.HasPassport;
        if (dto.PassportNumber != null) candidate.PassportNumber = string.IsNullOrWhiteSpace(dto.PassportNumber) ? null : dto.PassportNumber;
        if (dto.WorkAuthorization.HasValue) candidate.WorkAuthorization = dto.WorkAuthorization;
        if (dto.WorkAuthorizations != null) candidate.WorkAuthorizations = string.IsNullOrWhiteSpace(dto.WorkAuthorizations) ? null : dto.WorkAuthorizations;
        if (dto.HasTransport.HasValue) candidate.HasTransport = dto.HasTransport;
        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdateMigrationInfo", "PT_Candidates", candidate.Id, null, ipAddress);
        return true;
    }

    public async Task<List<VacancyOptionDto>> GetVacancyOptionsAsync()
    {
        return await _context.PT_Vacancies
            .Include(v => v.Company)
            .Where(v => !v.IsDeleted && v.Status == 0)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VacancyOptionDto
            {
                Id = v.Id,
                Title = v.Title,
                CompanyName = v.Company != null ? v.Company.Name : null,
                Location = v.Location,
                Description = v.Description,
                Requirements = v.Requirements,
                Category = v.Category,
                Status = v.Status
            })
            .ToListAsync();
    }

    public async Task<bool> LinkVacancyAsync(Guid recruitmentId, LinkVacancyDto dto, Guid adminId, string? ipAddress)
    {
        var recruitment = await _context.PT_CandidateRecruitments
            .FirstOrDefaultAsync(r => r.Id == recruitmentId && !r.IsDeleted);
        if (recruitment == null) return false;

        recruitment.PT_VacancyId = dto.VacancyId;
        recruitment.UpdatedAt = DateTime.UtcNow;
        recruitment.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "LinkVacancy", "PT_CandidateRecruitments", recruitmentId, null, ipAddress);
        return true;
    }
}
