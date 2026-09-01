using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.4, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub4.md (pesos, curva de experiencia, por que Idioma/Educacion quedan
/// fuera, etc).
/// </summary>
public class CompatibilityService : ICompatibilityService
{
    private readonly AppDbContext _context;

    // Defaults si la vacante no trae WeightsConfig propio (fase-3-sub4.md pregunta 1). Solo 3
    // dimensiones reales - Idioma/Educacion quedan fuera (pregunta 5).
    private const double DefaultSkillsWeight = 0.50;
    private const double DefaultExperienceWeight = 0.30;
    private const double DefaultLocationWeight = 0.20;

    // Anos minimos por bucket de ExperienceLevel (fase-3-sub4.md pregunta 3).
    private static readonly Dictionary<ExperienceLevel, int> ExperienceLevelMinYears = new()
    {
        [ExperienceLevel.Entry] = 0,
        [ExperienceLevel.Junior] = 1,
        [ExperienceLevel.Mid] = 3,
        [ExperienceLevel.Senior] = 5,
        [ExperienceLevel.Lead] = 8
    };

    private const int DefaultShortlistLimit = 20; // mismo default que AdminVacancyService.GetVacanciesAsync.

    public CompatibilityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobMatchDto> CalculateJobMatch(Guid candidateId, Guid vacancyId)
    {
        var candidate = await _context.PT_Candidates
            .Include(c => c.CandidateSkills)
            .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (candidate == null) throw new InvalidOperationException("Candidate not found");

        var vacancy = await _context.PT_Vacancies
            .Include(v => v.VacancySkills)
            .FirstOrDefaultAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (vacancy == null) throw new InvalidOperationException("Vacancy not found");

        var skillsMatch = CalculateSkillsMatch(candidate, vacancy);
        var experienceMatch = CalculateExperienceMatch(candidate, vacancy);
        var locationMatch = CalculateLocationMatch(candidate, vacancy);
        // EducationMatch reservado en 0 - ver fase-3-sub4.md pregunta 5.

        var match = await _context.PT_JobMatchScores
            .FirstOrDefaultAsync(m => m.PT_CandidateId == candidateId && m.PT_VacancyId == vacancyId && !m.IsDeleted);

        // PTVacancy no tiene un campo propio de pesos configurables (no esta en el esquema de
        // 3.1) - si esta fila ya tenia un WeightsConfig custom (seteado a mano o por una futura
        // feature de admin), se respeta en cada recalculo; si no, se usan los defaults y se
        // registran aca como "los pesos usados en este calculo" (fase-3-sub4.md pregunta 1).
        var weights = ParseWeights(match?.WeightsConfig);
        var percentage = (skillsMatch * weights.skills) + (experienceMatch * weights.experience) + (locationMatch * weights.location);

        if (match == null)
        {
            match = new PTJobMatchScore { PT_CandidateId = candidateId, PT_VacancyId = vacancyId };
            _context.PT_JobMatchScores.Add(match);
        }

        match.SkillsMatch = skillsMatch;
        match.ExperienceMatch = experienceMatch;
        match.LocationMatch = locationMatch;
        match.EducationMatch = 0;
        match.MatchPercentage = Math.Clamp((int)Math.Round(percentage), 0, 100);
        match.CalculatedAt = DateTime.UtcNow;
        match.UpdatedAt = DateTime.UtcNow;
        match.WeightsConfig ??= SerializeWeights(weights);

        await _context.SaveChangesAsync();

        return new JobMatchDto
        {
            CandidateId = candidateId,
            CandidateName = $"{candidate.FirstName} {candidate.LastName}".Trim(),
            CandidateTitle = candidate.Title,
            VacancyId = vacancyId,
            MatchPercentage = match.MatchPercentage,
            SkillsMatch = skillsMatch,
            ExperienceMatch = experienceMatch,
            EducationMatch = 0,
            LocationMatch = locationMatch,
            CalculatedAt = match.CalculatedAt
        };
    }

    public async Task<int> CalculateMatchesForVacancyAsync(Guid vacancyId)
    {
        var vacancyExists = await _context.PT_Vacancies.AnyAsync(v => v.Id == vacancyId && !v.IsDeleted);
        if (!vacancyExists) throw new InvalidOperationException("Vacancy not found");

        // Candidatos elegibles: perfil publico + wizard completo (mismo criterio que
        // AlertService para decidir que candidatos son visibles/notificables).
        var candidateIds = await _context.PT_Candidates
            .Where(c => !c.IsDeleted && c.IsProfilePublic && c.WizardCompleted)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var candidateId in candidateIds)
        {
            await CalculateJobMatch(candidateId, vacancyId);
        }

        return candidateIds.Count;
    }

    public async Task<List<JobMatchDto>> GenerateShortlist(Guid vacancyId, int? limit = null)
    {
        var take = limit is > 0 ? limit.Value : DefaultShortlistLimit;

        return await _context.PT_JobMatchScores
            .Where(m => m.PT_VacancyId == vacancyId && !m.IsDeleted)
            .Include(m => m.Candidate)
            .OrderByDescending(m => m.MatchPercentage)
            .Take(take)
            .Select(m => new JobMatchDto
            {
                CandidateId = m.PT_CandidateId,
                CandidateName = (m.Candidate.FirstName + " " + m.Candidate.LastName).Trim(),
                CandidateTitle = m.Candidate.Title,
                VacancyId = m.PT_VacancyId,
                MatchPercentage = m.MatchPercentage,
                SkillsMatch = m.SkillsMatch,
                ExperienceMatch = m.ExperienceMatch,
                EducationMatch = m.EducationMatch,
                LocationMatch = m.LocationMatch,
                CalculatedAt = m.CalculatedAt
            })
            .ToListAsync();
    }

    private static int CalculateSkillsMatch(PTCandidate candidate, PTVacancy vacancy)
    {
        var vacancySkills = vacancy.VacancySkills.Where(vs => !vs.IsDeleted).ToList();
        // Nada demandado por esta vacante puntual -> nada que fallar, 100% (distinto del
        // CompatibilityIndex agregado de la sub-fase 3.3, que usa 50 neutral a nivel mercado).
        if (vacancySkills.Count == 0) return 100;

        var candidateSkills = candidate.CandidateSkills.Where(cs => !cs.IsDeleted)
            .ToDictionary(cs => cs.PT_SkillId, cs => cs.ProficiencyLevel);

        double totalWeight = 0;
        double achieved = 0;

        foreach (var vs in vacancySkills)
        {
            // Las requeridas pesan el doble que las opcionales (fase-3-sub4.md pregunta 2).
            var weight = vs.IsRequired ? 2.0 : 1.0;
            totalWeight += weight;

            if (!candidateSkills.TryGetValue(vs.PT_SkillId, out var candidateLevel))
                continue; // no tiene la skill, no suma nada.

            if (vs.MinProficiencyLevel.HasValue && candidateLevel.HasValue && vs.MinProficiencyLevel.Value > 0)
            {
                // Ponderado por nivel cuando ambos lados lo cargan.
                var ratio = Math.Min(1.0, (double)candidateLevel.Value / vs.MinProficiencyLevel.Value);
                achieved += weight * ratio;
            }
            else
            {
                // Binario si falta el nivel de cualquiera de los dos lados.
                achieved += weight;
            }
        }

        return (int)Math.Round(achieved / totalWeight * 100);
    }

    private static int CalculateExperienceMatch(PTCandidate candidate, PTVacancy vacancy)
    {
        if (!vacancy.ExperienceLevel.HasValue) return 100; // sin requisito, neutral.

        var level = (ExperienceLevel)vacancy.ExperienceLevel.Value;
        var minYears = ExperienceLevelMinYears.GetValueOrDefault(level, 0);
        if (minYears == 0) return 100; // Entry no exige antiguedad.

        var candidateYears = candidate.YearsOfExperience ?? 0;
        if (candidateYears >= minYears) return 100;

        // Curva proporcional bajo el minimo (fase-3-sub4.md pregunta 3, ej. 3/5 = 60%).
        return (int)Math.Round((double)candidateYears / minYears * 100);
    }

    private static int CalculateLocationMatch(PTCandidate candidate, PTVacancy vacancy)
    {
        if (vacancy.WorkMode == (int)WorkMode.Remote) return 100;
        if (string.IsNullOrWhiteSpace(vacancy.Location)) return 100; // nada que comparar, neutral.

        var location = vacancy.Location.ToLowerInvariant();
        var cityMatch = !string.IsNullOrWhiteSpace(candidate.City) && location.Contains(candidate.City.ToLowerInvariant());
        var countryMatch = !string.IsNullOrWhiteSpace(candidate.Country) && location.Contains(candidate.Country.ToLowerInvariant());

        return (cityMatch || countryMatch) ? 100 : 0;
    }

    private static (double skills, double experience, double location) ParseWeights(string? weightsConfigJson)
    {
        double skills = DefaultSkillsWeight, experience = DefaultExperienceWeight, location = DefaultLocationWeight;

        if (!string.IsNullOrWhiteSpace(weightsConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(weightsConfigJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("skills", out var s) && s.TryGetDouble(out var sv)) skills = sv;
                if (root.TryGetProperty("experience", out var e) && e.TryGetDouble(out var ev)) experience = ev;
                if (root.TryGetProperty("location", out var l) && l.TryGetDouble(out var lv)) location = lv;
            }
            catch (JsonException)
            {
                // WeightsConfig invalido - se ignora y se usan los defaults, nunca lanza.
            }
        }

        // Se normaliza para que sumen 1, acepta tanto fracciones (0.5) como porcentajes (50).
        var total = skills + experience + location;
        if (total <= 0) return (DefaultSkillsWeight, DefaultExperienceWeight, DefaultLocationWeight);

        return (skills / total, experience / total, location / total);
    }

    private static string SerializeWeights((double skills, double experience, double location) weights)
        => JsonSerializer.Serialize(new { skills = weights.skills, experience = weights.experience, location = weights.location });
}
