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
        // Los candidatos Colocados no aparecen como disponibles (CandidatePlacementHelper).
        // Visibilidad RGPD (Fase 2, auditoria H-27): no basta IsProfilePublic - hace falta
        // el consentimiento registrado (VisibilityConsentAt) y que no este revocado despues.
        var placedIds = CandidatePlacementHelper.PlacedCandidateIds(_context);
        var query = _context.PT_Candidates
            .Where(c => !c.IsDeleted && c.IsProfilePublic && c.WizardCompleted
                && c.VisibilityConsentAt != null
                && (c.VisibilityConsentRevokedAt == null || c.VisibilityConsentRevokedAt < c.VisibilityConsentAt))
            .Where(c => !placedIds.Contains(c.Id));

        if (filter.SkillId.HasValue)
        {
            var skillId = filter.SkillId.Value;
            query = query.Where(c => c.CandidateSkills.Any(cs => !cs.IsDeleted && cs.PT_SkillId == skillId));
        }

        var candidates = await query
            .Select(c => new { c.Id, c.FirstName, c.LastName, c.Title, c.City, c.Country })
            .ToListAsync();

        // El score NO viaja a la empresa (H-27 / BE-19): es perfilado laboral automatizado
        // y hasta que no haya EIPD y base legal no se muestra ni se filtra por el. El estado
        // de verificacion se calcula siempre en vivo, nunca persistido (fase-3-sub7.md), y a
        // esta escala (docenas de candidatos, no miles) resolverlo en memoria es aceptable.
        var results = new List<CandidateSearchResultDto>();
        foreach (var c in candidates)
        {
            var status = await _verificationStatusService.GetVerificationStatusAsync(c.Id);
            if (filter.MinVerificationStatus.HasValue && status.Status < filter.MinVerificationStatus.Value)
                continue;

            results.Add(new CandidateSearchResultDto
            {
                CandidateId = c.Id,
                // Datos minimos (Fase 2 RGPD / embudo ciego): nombre + inicial del apellido.
                // La identidad completa solo viaja por la via de entrega (CanViewCandidateAsync).
                Name = MaskName(c.FirstName, c.LastName),
                Title = c.Title,
                City = c.City,
                Country = c.Country,
                VerificationStatus = status.Status,
                IsVerifiedTD = status.IsVerifiedTD
            });
        }

        results = results
            .OrderByDescending(r => r.VerificationStatus)
            .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
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
                && cs.Candidate.VisibilityConsentAt != null
                && (cs.Candidate.VisibilityConsentRevokedAt == null
                    || cs.Candidate.VisibilityConsentRevokedAt < cs.Candidate.VisibilityConsentAt)
                && cs.Candidate.WizardCompleted && !cs.Skill.IsDeleted)
            .Select(cs => new SkillOptionDto { Id = cs.PT_SkillId, Name = cs.Skill.Name })
            .Distinct()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>"María García" -> "María G."  Datos minimos: un apellido completo identifica
    /// demasiado en una busqueda agregada.</summary>
    private static string MaskName(string? firstName, string? lastName)
    {
        var first = (firstName ?? "").Trim();
        var initial = (lastName ?? "").Trim() is { Length: > 0 } last ? last[..1].ToUpperInvariant() + "." : "";
        return (first + " " + initial).Trim();
    }
}
