using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Services;

/// <summary>
/// Carga inicial de "Retos y competencias" desde Challenges/Seed/challenges-hosteleria.json (datos, no
/// codigo). Idempotente y sin sobrescribir:
/// - Competencias por Slug: solo se crean las que faltan.
/// - Cargos (catalogo PT_JobTypes) por nombre o alias: se crean los que faltan; en los existentes
///   solo se rellena la descripcion si esta vacia y las competencias si no tiene ninguna.
/// - Retos por Slug: si ya existe (aunque se haya editado o archivado) no se toca.
/// - Los retos nuevos se publican (version 1) solo si pasan la validacion; si no, quedan en borrador.
/// En el JSON las competencias se referencian como "comp:slug" y se traducen al Id real.
/// </summary>
internal class ChallengeSeeder
{
    private const string ResourceName = "OpenToWork.Core.Challenges.Seed.challenges-hosteleria.json";

    private readonly AppDbContext _context;
    private readonly IAuditLogService _audit;

    public ChallengeSeeder(AppDbContext context, IAuditLogService audit)
    {
        _context = context;
        _audit = audit;
    }

    private class SeedFile
    {
        public List<SeedCompetency> Competencies { get; set; } = new();
        public List<SeedJobType> JobTypes { get; set; } = new();
        public List<SeedChallenge> Challenges { get; set; } = new();
    }
    private class SeedCompetency { public string Slug { get; set; } = ""; public string Name { get; set; } = ""; public string Description { get; set; } = ""; }
    private class SeedJobType
    {
        public string Name { get; set; } = "";
        public List<string> Aliases { get; set; } = new();
        public string LevelName { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Competencies { get; set; } = new();
    }
    private class SeedChallenge
    {
        public string Slug { get; set; } = "";
        public List<string> JobTypes { get; set; } = new();
        public System.Text.Json.JsonElement Definition { get; set; }
    }

    public static string LoadSeedJson()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"No se encontro el recurso {ResourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public async Task<SeedResultDto> SeedAsync(Guid adminId)
    {
        var result = new SeedResultDto();
        var seed = ChallengeJson.Deserialize<SeedFile>(LoadSeedJson());

        // 1. Competencias
        var competencies = await _context.PT_Competencies.Where(c => !c.IsDeleted).ToListAsync();
        var order = competencies.Count == 0 ? 0 : competencies.Max(c => c.SortOrder) + 1;
        foreach (var sc in seed.Competencies)
        {
            if (competencies.Any(c => c.Slug == sc.Slug)) continue;
            var c = new PTCompetency { Slug = sc.Slug, Name = sc.Name, Description = sc.Description, SortOrder = order++, CreatedBy = adminId };
            _context.PT_Competencies.Add(c);
            competencies.Add(c);
            result.CompetenciesCreated++;
        }
        await _context.SaveChangesAsync();
        var bySlug = competencies.Where(c => c.Slug != null).ToDictionary(c => c.Slug!, c => c.Id);

        // 2. Cargos (catalogo existente)
        var types = await _context.PT_JobTypes.Where(t => !t.IsDeleted).ToListAsync();
        var levels = await _context.PT_JobLevels.Where(l => !l.IsDeleted).OrderBy(l => l.SortOrder).ToListAsync();
        var typeIds = new Dictionary<string, Guid>();
        foreach (var sj in seed.JobTypes)
        {
            var names = sj.Aliases.Append(sj.Name).Select(n => n.Trim().ToLowerInvariant()).ToHashSet();
            var type = types.FirstOrDefault(t => names.Contains(t.Name.Trim().ToLowerInvariant()));
            if (type == null)
            {
                var level = levels.FirstOrDefault(l => l.Name == sj.LevelName) ?? levels.LastOrDefault();
                if (level == null) { result.Errors.Add($"No hay niveles de puesto para crear '{sj.Name}'."); continue; }
                type = new PTJobType
                {
                    Name = sj.Name, PT_JobLevelId = level.Id, Description = sj.Description, IsActive = true,
                    SortOrder = types.Where(t => t.PT_JobLevelId == level.Id).Select(t => t.SortOrder).DefaultIfEmpty(0).Max() + 1,
                    CreatedBy = adminId
                };
                _context.PT_JobTypes.Add(type);
                types.Add(type);
                result.JobTypesCreated++;
            }
            else if (string.IsNullOrWhiteSpace(type.Description))
            {
                type.Description = sj.Description;
            }
            typeIds[sj.Name] = type.Id;

            var hasLinks = await _context.PT_JobTypeCompetencies.AnyAsync(x => x.PT_JobTypeId == type.Id && !x.IsDeleted)
                           || _context.ChangeTracker.Entries<PTJobTypeCompetency>().Any(e => e.Entity.PT_JobTypeId == type.Id);
            if (!hasLinks)
                foreach (var slug in sj.Competencies.Where(bySlug.ContainsKey))
                    _context.PT_JobTypeCompetencies.Add(new PTJobTypeCompetency { PT_JobTypeId = type.Id, PT_CompetencyId = bySlug[slug], CreatedBy = adminId });
        }
        await _context.SaveChangesAsync();

        // 3. Retos
        var activeIds = competencies.Where(c => c.IsActive).Select(c => c.Id).ToHashSet();
        foreach (var sch in seed.Challenges)
        {
            if (await _context.PT_Challenges.AnyAsync(c => c.Slug == sch.Slug)) { result.ChallengesSkipped++; continue; }

            var json = sch.Definition.GetRawText();
            foreach (var kv in bySlug) json = json.Replace($"\"comp:{kv.Key}\"", $"\"{kv.Value}\"");
            if (json.Contains("\"comp:")) { result.Errors.Add($"{sch.Slug}: referencia a una competencia que no existe."); continue; }
            var def = ChallengeJson.Deserialize<ChallengeDefinition>(json);

            var challenge = new PTChallenge
            {
                Slug = sch.Slug, Title = def.Title, Status = (int)ChallengeStatus.Draft,
                DraftJson = ChallengeJson.Serialize(def), HasDraftChanges = true, CreatedBy = adminId
            };
            var i = 0;
            foreach (var jt in sch.JobTypes.Where(typeIds.ContainsKey))
                challenge.JobTypes.Add(new PTChallengeJobType { PT_ChallengeId = challenge.Id, PT_JobTypeId = typeIds[jt], SortOrder = i++, CreatedBy = adminId });
            _context.PT_Challenges.Add(challenge);
            result.ChallengesCreated++;

            var errors = ChallengeRules.Validate(def, activeIds, challenge.JobTypes.Count);
            if (errors.Count > 0)
            {
                result.Errors.AddRange(errors.Select(e => $"{sch.Slug}: {e}"));
            }
            else
            {
                var version = new PTChallengeVersion
                {
                    PT_ChallengeId = challenge.Id, VersionNumber = 1, ContentJson = challenge.DraftJson,
                    PublishedAt = DateTime.UtcNow, PublishedBy = adminId, CreatedBy = adminId
                };
                _context.PT_ChallengeVersions.Add(version);
                challenge.Status = (int)ChallengeStatus.Published;
                challenge.LatestVersionId = version.Id;
                challenge.LatestVersionNumber = 1;
                challenge.HasDraftChanges = false;
                challenge.PublishedAt = version.PublishedAt;
                challenge.PublishedBy = adminId;
                result.ChallengesPublished++;
            }
            await _context.SaveChangesAsync();
            await _audit.LogAsync(adminId, "ChallengeSeeded", "PT_Challenges", challenge.Id, sch.Slug, null);
        }
        return result;
    }
}
