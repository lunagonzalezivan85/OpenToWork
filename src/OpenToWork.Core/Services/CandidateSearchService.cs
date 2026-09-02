using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver Fase 5 (Portal Corporativo) en README - busqueda avanzada por score/verificacion/skill,
/// item que quedaba pendiente de Fase 3.
/// </summary>
public class CandidateSearchService : ICandidateSearchService
{
    private readonly AppDbContext _context;
    private readonly IVerificationStatusService _verificationStatusService;

    public CandidateSearchService(AppDbContext context, IVerificationStatusService verificationStatusService)
    {
        _context = context;
        _verificationStatusService = verificationStatusService;
    }

    public async Task<CandidateSearchResultPageDto> SearchAsync(CandidateSearchFilterDto filter)
    {
        var query = _context.PT_Candidates
            .Where(c => !c.IsDeleted && c.IsProfilePublic && c.WizardCompleted);

        if (filter.SkillId.HasValue)
        {
            var skillId = filter.SkillId.Value;
            query = query.Where(c => c.CandidateSkills.Any(cs => !cs.IsDeleted && cs.PT_SkillId == skillId));
        }

        var candidates = await query
            .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName, c.Title, c.City, c.Country })
            .ToListAsync();

        // Score/estado se computan por candidato (no son filtrables via SQL directo: el score
        // vive en una tabla aparte con upsert diferido, y el estado de verificacion se calcula
        // siempre en vivo, nunca persistido - fase-3-sub7.md). A esta escala (docenas de
        // candidatos, no miles) resolver en memoria es aceptable; si el volumen crece, esto
        // necesitaria un job que materialice snapshots para filtrar/paginar en SQL.
        var candidateIds = candidates.Select(c => c.Id).ToList();
        var scores = await _context.PT_CandidateScores
            .Where(s => !s.IsDeleted && candidateIds.Contains(s.PT_CandidateId))
            .ToDictionaryAsync(s => s.PT_CandidateId, s => s.OverallScore);

        var results = new List<CandidateSearchResultDto>();
        foreach (var c in candidates)
        {
            var overallScore = scores.GetValueOrDefault(c.Id, 0);
            if (filter.MinOverallScore.HasValue && overallScore < filter.MinOverallScore.Value)
                continue;

            var status = await _verificationStatusService.GetVerificationStatusAsync(c.Id);
            if (filter.MinVerificationStatus.HasValue && status.Status < filter.MinVerificationStatus.Value)
                continue;

            results.Add(new CandidateSearchResultDto
            {
                CandidateId = c.Id,
                Name = c.Name.Trim(),
                Title = c.Title,
                City = c.City,
                Country = c.Country,
                OverallScore = overallScore,
                VerificationStatus = status.Status,
                IsVerifiedTD = status.IsVerifiedTD
            });
        }

        results = results
            .OrderByDescending(r => r.OverallScore)
            .ToList();

        var total = results.Count;
        var page = Math.Max(1, filter.Page);
        var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

        var pageItems = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new CandidateSearchResultPageDto
        {
            Items = pageItems,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<SkillOptionDto>> GetSearchableSkillsAsync()
    {
        return await _context.PT_CandidateSkills
            .Where(cs => !cs.IsDeleted && !cs.Candidate.IsDeleted && cs.Candidate.IsProfilePublic
                && cs.Candidate.WizardCompleted && !cs.Skill.IsDeleted)
            .Select(cs => new SkillOptionDto { Id = cs.PT_SkillId, Name = cs.Skill.Name })
            .Distinct()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}
