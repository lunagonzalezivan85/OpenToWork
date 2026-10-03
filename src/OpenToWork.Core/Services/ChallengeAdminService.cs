using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Services;

/// <summary>
/// "Retos y competencias" del admin. El borrador vive en PT_Challenges.DraftJson; publicar congela
/// una copia en PT_ChallengeVersions (inmutable). Los candidatos solo ven versiones publicadas.
/// Nada referenciado por intentos se borra: competencias y retos se desactivan o archivan.
/// </summary>
public class ChallengeAdminService : IChallengeAdminService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _audit;

    public ChallengeAdminService(AppDbContext context, IAuditLogService audit)
    {
        _context = context;
        _audit = audit;
    }

    // ---------------- Competencias ----------------

    public async Task<List<CompetencyDto>> GetCompetenciesAsync()
    {
        var list = await _context.PT_Competencies.Where(c => !c.IsDeleted)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();
        var drafts = await _context.PT_Challenges.Where(c => !c.IsDeleted).Select(c => c.DraftJson).ToListAsync();
        var used = drafts.Select(ChallengeJson.Deserialize<ChallengeDefinition>).Select(UsedCompetencies).ToList();
        return list.Select(c => new CompetencyDto
        {
            Id = c.Id, Name = c.Name, Description = c.Description, IsActive = c.IsActive, SortOrder = c.SortOrder,
            UsedInChallenges = used.Count(u => u.Contains(c.Id))
        }).ToList();
    }

    public async Task<ChallengeActionResultDto> CreateCompetencyAsync(SaveCompetencyDto dto, Guid adminId)
    {
        var error = ValidateCompetency(dto);
        if (error != null) return Fail(error);
        if (await _context.PT_Competencies.AnyAsync(c => !c.IsDeleted && c.Name == dto.Name.Trim()))
            return Fail("Ya existe una competencia con ese nombre.");

        var c = new PTCompetency
        {
            Name = dto.Name.Trim(), Description = dto.Description?.Trim(), IsActive = dto.IsActive,
            SortOrder = dto.SortOrder, CreatedBy = adminId
        };
        _context.PT_Competencies.Add(c);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "CompetencyCreated", "PT_Competencies", c.Id, c.Name, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public async Task<ChallengeActionResultDto> UpdateCompetencyAsync(Guid id, SaveCompetencyDto dto, Guid adminId)
    {
        var error = ValidateCompetency(dto);
        if (error != null) return Fail(error);
        var c = await _context.PT_Competencies.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("La competencia no existe.");
        if (await _context.PT_Competencies.AnyAsync(x => !x.IsDeleted && x.Id != id && x.Name == dto.Name.Trim()))
            return Fail("Ya existe una competencia con ese nombre.");

        // Las versiones publicadas guardan el Id: cambiar el nombre se refleja en resultados antiguos,
        // pero desactivar no los rompe (se siguen mostrando con su nombre).
        c.Name = dto.Name.Trim();
        c.Description = dto.Description?.Trim();
        c.IsActive = dto.IsActive;
        c.SortOrder = dto.SortOrder;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = adminId;
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "CompetencyUpdated", "PT_Competencies", c.Id, c.Name, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    private static string? ValidateCompetency(SaveCompetencyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return "El nombre es obligatorio.";
        if (dto.Name.Trim().Length > 100) return "El nombre no puede superar 100 caracteres.";
        if ((dto.Description?.Length ?? 0) > 500) return "La descripcion no puede superar 500 caracteres.";
        return null;
    }

    // ---------------- Cargos (catalogo existente PT_JobTypes) ----------------

    public async Task<List<ChallengeJobTypeConfigDto>> GetJobTypesAsync()
    {
        var types = await _context.PT_JobTypes.Include(t => t.JobLevel)
            .Where(t => !t.IsDeleted).OrderBy(t => t.JobLevel.SortOrder).ThenBy(t => t.SortOrder).ToListAsync();
        var comps = await _context.PT_JobTypeCompetencies.Where(x => !x.IsDeleted).ToListAsync();
        var links = await _context.PT_ChallengeJobTypes.Include(x => x.Challenge)
            .Where(x => !x.IsDeleted && !x.Challenge.IsDeleted).ToListAsync();

        return types.Select(t => new ChallengeJobTypeConfigDto
        {
            Id = t.Id, Name = t.Name, LevelName = t.JobLevel?.Name, Description = t.Description,
            IsActive = t.IsActive, SortOrder = t.SortOrder,
            CompetencyIds = comps.Where(c => c.PT_JobTypeId == t.Id).Select(c => c.PT_CompetencyId).ToList(),
            ChallengeTitles = links.Where(l => l.PT_JobTypeId == t.Id).OrderBy(l => l.SortOrder).Select(l => l.Challenge.Title).ToList()
        }).ToList();
    }

    public async Task<ChallengeActionResultDto> SaveJobTypeAsync(Guid jobTypeId, SaveChallengeJobTypeDto dto, Guid adminId)
    {
        var type = await _context.PT_JobTypes.FirstOrDefaultAsync(t => t.Id == jobTypeId && !t.IsDeleted);
        if (type == null) return Fail("El cargo no existe.");
        if ((dto.Description?.Length ?? 0) > 500) return Fail("La descripcion no puede superar 500 caracteres.");
        var validComps = await _context.PT_Competencies.Where(c => !c.IsDeleted && dto.CompetencyIds.Contains(c.Id)).Select(c => c.Id).ToListAsync();

        type.Description = dto.Description?.Trim();
        type.UpdatedAt = DateTime.UtcNow;
        type.UpdatedBy = adminId;

        var current = await _context.PT_JobTypeCompetencies.Where(x => x.PT_JobTypeId == jobTypeId && !x.IsDeleted).ToListAsync();
        foreach (var link in current.Where(l => !validComps.Contains(l.PT_CompetencyId)))
            _context.PT_JobTypeCompetencies.Remove(link); // relacion sin historico propio: se borra
        foreach (var compId in validComps.Where(id => current.All(l => l.PT_CompetencyId != id)))
            _context.PT_JobTypeCompetencies.Add(new PTJobTypeCompetency { PT_JobTypeId = jobTypeId, PT_CompetencyId = compId, CreatedBy = adminId });

        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeJobTypeUpdated", "PT_JobTypes", jobTypeId, null, null);
        return new ChallengeActionResultDto { Success = true, Id = jobTypeId };
    }

    // ---------------- Retos ----------------

    public async Task<List<ChallengeListItemDto>> GetChallengesAsync()
    {
        var list = await _context.PT_Challenges
            .Include(c => c.JobTypes.Where(j => !j.IsDeleted)).ThenInclude(j => j.JobType)
            .Where(c => !c.IsDeleted).OrderBy(c => c.Title).ToListAsync();
        var attempts = await _context.PT_ChallengeAttempts.Where(a => !a.IsDeleted)
            .GroupBy(a => a.PT_ChallengeId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

        return list.Select(c => new ChallengeListItemDto
        {
            Id = c.Id, Slug = c.Slug, Title = c.Title, Status = (ChallengeStatus)c.Status,
            HasDraftChanges = c.HasDraftChanges, LatestVersion = c.LatestVersionNumber,
            JobTypes = c.JobTypes.OrderBy(j => j.SortOrder).Select(j => j.JobType.Name).ToList(),
            ActivityCount = ChallengeJson.Deserialize<ChallengeDefinition>(c.DraftJson).Activities.Count,
            AttemptCount = attempts.FirstOrDefault(a => a.Key == c.Id)?.Count ?? 0,
            UpdatedAt = c.UpdatedAt ?? c.CreatedAt
        }).ToList();
    }

    public async Task<ChallengeEditDto?> GetChallengeAsync(Guid id)
    {
        var c = await _context.PT_Challenges.Include(x => x.JobTypes)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return null;

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(c.DraftJson);
        var jobTypeIds = c.JobTypes.Where(j => !j.IsDeleted).OrderBy(j => j.SortOrder).Select(j => j.PT_JobTypeId).ToList();
        var attemptCount = await _context.PT_ChallengeAttempts.CountAsync(a => a.PT_ChallengeId == id && !a.IsDeleted);

        var history = await _context.AD_AuditLogs.Include(a => a.User)
            .Where(a => !a.IsDeleted && a.EntityType == "PT_Challenges" && a.EntityId == id)
            .OrderByDescending(a => a.CreatedAt).Take(50)
            .Select(a => new ChallengeHistoryEntryDto { At = a.CreatedAt, Action = a.Action, By = a.User.FullName ?? a.User.Email })
            .ToListAsync();

        return new ChallengeEditDto
        {
            Id = c.Id, Slug = c.Slug, Status = (ChallengeStatus)c.Status, HasDraftChanges = c.HasDraftChanges,
            LatestVersion = c.LatestVersionNumber, PublishedAt = c.PublishedAt, ArchivedAt = c.ArchivedAt,
            JobTypeIds = jobTypeIds, Definition = def, AttemptCount = attemptCount,
            CanDelete = c.LatestVersionNumber == null && attemptCount == 0,
            ValidationErrors = ChallengeRules.Validate(def, await ActiveCompetencyIdsAsync(), jobTypeIds.Count),
            History = history
        };
    }

    public async Task<ChallengeActionResultDto> CreateChallengeAsync(SaveChallengeDto dto, Guid adminId)
    {
        var shapeError = ValidateShape(dto.Definition);
        if (shapeError != null) return Fail(shapeError);

        var c = new PTChallenge
        {
            Slug = await UniqueSlugAsync(dto.Definition.Title),
            Title = Truncate(dto.Definition.Title.Trim(), 200),
            Status = (int)ChallengeStatus.Draft,
            DraftJson = ChallengeJson.Serialize(dto.Definition),
            HasDraftChanges = true,
            CreatedBy = adminId
        };
        _context.PT_Challenges.Add(c);
        await SetJobTypesAsync(c, dto.JobTypeIds, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeCreated", "PT_Challenges", c.Id, c.Title, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public async Task<ChallengeActionResultDto> SaveDraftAsync(Guid id, SaveChallengeDto dto, Guid adminId)
    {
        var c = await _context.PT_Challenges.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("El reto no existe.");
        var shapeError = ValidateShape(dto.Definition);
        if (shapeError != null) return Fail(shapeError);

        // Editar un reto publicado no cambia lo que ven los candidatos: se publica como nueva version.
        c.Title = Truncate(dto.Definition.Title.Trim(), 200);
        c.DraftJson = ChallengeJson.Serialize(dto.Definition);
        c.HasDraftChanges = true;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = adminId;
        await SetJobTypesAsync(c, dto.JobTypeIds, adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeDraftSaved", "PT_Challenges", c.Id, null, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public async Task<ChallengeActionResultDto> DuplicateAsync(Guid id, Guid adminId)
    {
        var src = await _context.PT_Challenges.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (src == null) return Fail("El reto no existe.");

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(src.DraftJson);
        def.Title = Truncate($"{def.Title} (copia)", 200);
        var copy = new PTChallenge
        {
            Slug = await UniqueSlugAsync(def.Title),
            Title = def.Title,
            Status = (int)ChallengeStatus.Draft,
            DraftJson = ChallengeJson.Serialize(def),
            HasDraftChanges = true,
            CreatedBy = adminId
        };
        _context.PT_Challenges.Add(copy);
        await SetJobTypesAsync(copy, src.JobTypes.Where(j => !j.IsDeleted).OrderBy(j => j.SortOrder).Select(j => j.PT_JobTypeId).ToList(), adminId);
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeDuplicated", "PT_Challenges", copy.Id, $"desde {src.Id}", null);
        return new ChallengeActionResultDto { Success = true, Id = copy.Id };
    }

    public async Task<ChallengeActionResultDto> DeleteAsync(Guid id, Guid adminId)
    {
        var c = await _context.PT_Challenges.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("El reto no existe.");
        if (c.LatestVersionNumber != null || await _context.PT_ChallengeAttempts.AnyAsync(a => a.PT_ChallengeId == id))
            return Fail("Solo se pueden eliminar borradores que nunca se han publicado ni usado. Archivalo en su lugar.");

        // Borrado logico (patron del proyecto): no aparece en ningun listado.
        c.IsDeleted = true;
        c.DeletedAt = DateTime.UtcNow;
        c.DeletedBy = adminId;
        foreach (var j in c.JobTypes) { j.IsDeleted = true; j.DeletedAt = DateTime.UtcNow; j.DeletedBy = adminId; }
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeDeleted", "PT_Challenges", c.Id, c.Title, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public async Task<ChallengeActionResultDto> PublishAsync(Guid id, Guid adminId)
    {
        var c = await _context.PT_Challenges.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("El reto no existe.");

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(c.DraftJson);
        var errors = ChallengeRules.Validate(def, await ActiveCompetencyIdsAsync(), c.JobTypes.Count(j => !j.IsDeleted));
        if (errors.Count > 0) return new ChallengeActionResultDto { Success = false, Errors = errors };
        if (!c.HasDraftChanges && c.Status == (int)ChallengeStatus.Published)
            return Fail("No hay cambios sin publicar.");

        var number = (c.LatestVersionNumber ?? 0) + 1;
        var version = new PTChallengeVersion
        {
            PT_ChallengeId = c.Id,
            VersionNumber = number,
            ContentJson = c.DraftJson, // copia congelada: los intentos siempre leen de aqui
            PublishedAt = DateTime.UtcNow,
            PublishedBy = adminId,
            CreatedBy = adminId
        };
        _context.PT_ChallengeVersions.Add(version);
        c.LatestVersionId = version.Id;
        c.LatestVersionNumber = number;
        c.Status = (int)ChallengeStatus.Published;
        c.HasDraftChanges = false;
        c.PublishedAt = version.PublishedAt;
        c.PublishedBy = adminId;
        c.ArchivedAt = null;
        c.ArchivedBy = null;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = adminId;
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengePublished", "PT_Challenges", c.Id, $"version {number}", null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id, VersionNumber = number };
    }

    public async Task<ChallengeActionResultDto> ArchiveAsync(Guid id, Guid adminId)
    {
        var c = await _context.PT_Challenges.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("El reto no existe.");
        if (c.Status == (int)ChallengeStatus.Archived) return Fail("El reto ya esta archivado.");

        // Sin nuevos intentos; los iniciados se pueden terminar con su version.
        c.Status = (int)ChallengeStatus.Archived;
        c.ArchivedAt = DateTime.UtcNow;
        c.ArchivedBy = adminId;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = adminId;
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeArchived", "PT_Challenges", c.Id, null, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public async Task<ChallengeActionResultDto> RestoreAsync(Guid id, Guid adminId)
    {
        var c = await _context.PT_Challenges.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return Fail("El reto no existe.");
        if (c.Status != (int)ChallengeStatus.Archived) return Fail("El reto no esta archivado.");

        // Vuelve a ofrecerse la ultima version publicada; si nunca se publico queda como borrador.
        c.Status = c.LatestVersionNumber != null ? (int)ChallengeStatus.Published : (int)ChallengeStatus.Draft;
        c.ArchivedAt = null;
        c.ArchivedBy = null;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = adminId;
        await _context.SaveChangesAsync();
        await _audit.LogAsync(adminId, "ChallengeRestored", "PT_Challenges", c.Id, null, null);
        return new ChallengeActionResultDto { Success = true, Id = c.Id };
    }

    public PracticeFeedbackDto PreviewEvaluate(PreviewEvaluateDto dto)
    {
        var clean = ChallengeScoring.Sanitize(dto.Activity, dto.Response);
        var auto = ChallengeScoring.ScoreAuto(dto.Activity, clean);
        return new PracticeFeedbackDto
        {
            AutoPercent = auto == null ? null : Math.Round(auto.Value * 100, 1),
            Explanation = dto.Activity.PracticeExplanation,
            HasHumanPart = ChallengeRules.HasHumanPart(dto.Activity.ResponseType)
        };
    }

    public Task<SeedResultDto> SeedInitialContentAsync(Guid adminId) =>
        new ChallengeSeeder(_context, _audit).SeedAsync(adminId);

    // ---------------- Auxiliares ----------------

    internal async Task<HashSet<Guid>> ActiveCompetencyIdsAsync() =>
        (await _context.PT_Competencies.Where(c => !c.IsDeleted && c.IsActive).Select(c => c.Id).ToListAsync()).ToHashSet();

    private async Task SetJobTypesAsync(PTChallenge c, List<Guid> jobTypeIds, Guid adminId)
    {
        var valid = await _context.PT_JobTypes.Where(t => !t.IsDeleted && jobTypeIds.Contains(t.Id)).Select(t => t.Id).ToListAsync();
        var ordered = jobTypeIds.Where(valid.Contains).Distinct().ToList();
        var current = c.JobTypes.Where(j => !j.IsDeleted).ToList();

        foreach (var link in current.Where(l => !ordered.Contains(l.PT_JobTypeId)))
        {
            link.IsDeleted = true;
            link.DeletedAt = DateTime.UtcNow;
            link.DeletedBy = adminId;
        }
        for (var i = 0; i < ordered.Count; i++)
        {
            var existing = current.FirstOrDefault(l => l.PT_JobTypeId == ordered[i]);
            if (existing != null) existing.SortOrder = i;
            else c.JobTypes.Add(new PTChallengeJobType { PT_ChallengeId = c.Id, PT_JobTypeId = ordered[i], SortOrder = i, CreatedBy = adminId });
        }
    }

    /// <summary>Limites basicos para guardar un borrador (la validacion completa es al publicar).</summary>
    private static string? ValidateShape(ChallengeDefinition d)
    {
        if (string.IsNullOrWhiteSpace(d.Title)) return "El titulo es obligatorio para guardar.";
        if (d.Activities.Count > 30) return "Un reto no puede tener mas de 30 actividades.";
        if (d.Activities.Any(a => a.Options.Count > 20 || a.Resources.Count > 10)) return "Demasiadas opciones o recursos en una actividad.";
        var json = ChallengeJson.Serialize(d);
        if (json.Length > 500_000) return "El reto es demasiado grande.";
        return null;
    }

    private async Task<string> UniqueSlugAsync(string title)
    {
        var baseSlug = Slugify(title);
        var slug = baseSlug;
        var i = 2;
        while (await _context.PT_Challenges.AnyAsync(c => c.Slug == slug)) slug = $"{baseSlug}-{i++}";
        return slug;
    }

    internal static string Slugify(string text)
    {
        var normalized = text.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var ch in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }
        var slug = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        if (slug.Length > 60) slug = slug[..60].Trim('-');
        return string.IsNullOrEmpty(slug) ? "reto" : slug;
    }

    internal static HashSet<Guid> UsedCompetencies(ChallengeDefinition d) =>
        d.Activities.SelectMany(a => a.AutoCompetencies.Select(c => c.CompetencyId).Concat(a.Rubric.Select(r => r.CompetencyId))).ToHashSet();

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    private static ChallengeActionResultDto Fail(string error) => new() { Success = false, Errors = new() { error } };
}
