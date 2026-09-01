using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

/// <summary>
/// Ver plan obligatorio de Fase 3 en README, sub-fase 3.3, y las decisiones documentadas en
/// docs/dsiezar/fase-3-sub3.md (pesos, umbrales, que pasa sin experiencias/vacantes, etc).
/// </summary>
public class ScoringService : IScoringService
{
    private readonly AppDbContext _context;

    // Pesos del OverallScore (fase-3-sub3.md pregunta 1).
    private const double StabilityWeight = 0.30;
    private const double ReliabilityWeight = 0.25;
    private const double EvidenceWeight = 0.25;
    private const double CompatibilityWeight = 0.20;

    // Duracion promedio con techo en 5 anos = 100 (pregunta 2).
    private const int StabilityDurationCapMonths = 60;
    // Mas de 1 cambio de empleo por ano se considera "frecuente" (pregunta 3).
    private const double FrequentChangesPerYearThreshold = 1.0;
    private const int ShortStintMonthsThreshold = 3; // mismo umbral que ValidationService (3.2).
    private const int ShortStintPenalty = 15;
    private const int CurrentJobBonusMonths = 12;
    private const int CurrentJobBonus = 10;

    // Gap/overlap: misma formula que ValidationService.VerifyCvCoherenceAsync (3.2 pregunta 3/4),
    // para no tener dos criterios distintos de "coherencia cronologica" en el sistema.
    private const int GapMonthsThreshold = 6;
    private const int OverlapDaysThreshold = 30;

    // Penalizacion maxima por skills del candidato que ninguna vacante activa demanda (pregunta 1 de 3.4 no aplica aca; ver sub3 pregunta sobre compatibilidad).
    private const int UnusedSkillsMaxPenalty = 20;

    // EvidenceIndex: pesos por componente, re-pesados en la sub-fase 3.6 al sumar SkillTest
    // (fase-3-sub6.md pregunta 6) - suman 100 entre los 5 de PT_Verifications + SkillTest.
    private static readonly (VerificationType type, int weight)[] EvidenceVerificationWeights =
    {
        (VerificationType.LinkedIn, 15),
        (VerificationType.Portfolio, 15),
        (VerificationType.CvCoherence, 20),
        (VerificationType.Identity, 15),
        (VerificationType.Reference, 15)
    };
    private const int EvidenceSkillTestWeight = 20;

    public ScoringService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculateStabilityIndex(Guid candidateId)
    {
        var experiences = await _context.PT_CandidateExperiences
            .Where(e => e.PT_CandidateId == candidateId && !e.IsDeleted)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        // Sin experiencias no hay evidencia de estabilidad que evaluar (fase-3-sub3.md pregunta 12).
        if (experiences.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var durations = experiences.Select(e => MonthsBetween(e.StartDate, ResolveEnd(e.StartDate, e.EndDate, e.IsCurrentJob, now))).ToList();
        var avgDurationMonths = durations.Average();

        // Lineal 0-60 meses -> 0-100, tope en 60+ (pregunta 2).
        var durationScore = Math.Min(100, avgDurationMonths / StabilityDurationCapMonths * 100);

        var spanStart = experiences.Min(e => e.StartDate);
        var spanEnd = experiences.Max(e => ResolveEnd(e.StartDate, e.EndDate, e.IsCurrentJob, now));
        var spanYears = Math.Max(1.0, (spanEnd - spanStart).TotalDays / 365.0);
        var changesPerYear = experiences.Count / spanYears;

        // Penalizacion por cambios frecuentes (pregunta 3).
        var frequencyPenalty = changesPerYear > FrequentChangesPerYearThreshold
            ? Math.Min(30, (changesPerYear - FrequentChangesPerYearThreshold) * 15)
            : 0;

        // Penalizacion por empleos cortos (< 3 meses, excluyendo el actual).
        var shortStintPenalty = experiences.Count(e => !e.IsCurrentJob &&
            MonthsBetween(e.StartDate, ResolveEnd(e.StartDate, e.EndDate, e.IsCurrentJob, now)) < ShortStintMonthsThreshold) * ShortStintPenalty;

        // Bonus por empleo actual > 12 meses.
        var current = experiences.FirstOrDefault(e => e.IsCurrentJob);
        var currentBonus = current != null && MonthsBetween(current.StartDate, now) > CurrentJobBonusMonths ? CurrentJobBonus : 0;

        var score = durationScore - frequencyPenalty - shortStintPenalty + currentBonus;
        return Math.Clamp((int)Math.Round(score), 0, 100);
    }

    public async Task<int> CalculateReliabilityIndex(Guid candidateId)
    {
        var experiences = await _context.PT_CandidateExperiences
            .Where(e => e.PT_CandidateId == candidateId && !e.IsDeleted)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        // Sin experiencias (o solo una) no hay cronologia que pueda ser incoherente: 100 por
        // definicion ("sin gaps ni superposiciones = 100", fase-3-sub3.md pregunta 4).
        if (experiences.Count < 2) return 100;

        var now = DateTime.UtcNow;
        var score = 100.0;

        for (int i = 1; i < experiences.Count; i++)
        {
            var prev = experiences[i - 1];
            var curr = experiences[i];
            var prevEnd = ResolveEnd(prev.StartDate, prev.EndDate, prev.IsCurrentJob, now);
            var gapDays = (curr.StartDate - prevEnd).TotalDays;

            if (gapDays > GapMonthsThreshold * 30)
            {
                // Proporcional a la duracion del gap, mas peso si es reciente (pregunta 4,
                // misma formula que ValidationService 3.2).
                var gapMonths = gapDays / 30;
                var isRecent = (now - prevEnd).TotalDays < 730;
                score -= Math.Min(30, 10 + gapMonths / 2) * (isRecent ? 1.5 : 1.0);
            }
            else if (gapDays < -OverlapDaysThreshold)
            {
                score -= 10;
            }
        }

        // Nota: no hay bonus por "progresion logica" (ascensos, misma industria) - se decidio
        // no implementar una heuristica de texto poco confiable sobre JobTitle sin datos
        // estructurados de seniority/industria (fase-3-sub3.md pregunta 5).

        return Math.Clamp((int)Math.Round(score), 0, 100);
    }

    public async Task<int> CalculateEvidenceIndex(Guid candidateId)
    {
        // Lee los resultados ya persistidos por ValidationService - no dispara verificaciones
        // HTTP nuevas aca (esas corren bajo demanda via POST verifications/run, fase-3-sub2.md).
        var verifications = await _context.PT_Verifications
            .Where(v => v.PT_CandidateId == candidateId && !v.IsDeleted)
            .ToListAsync();

        // Re-pesado a 6 componentes en la sub-fase 3.6 (fase-3-sub6.md pregunta 6): los checks
        // mas sustantivos (coherencia de todo el CV, reto de habilidades aprobado) pesan mas
        // que los binarios simples (URL, referencia unica, stub de identidad). Suma 100.
        var score = 0;
        foreach (var (type, weight) in EvidenceVerificationWeights)
        {
            var v = verifications.FirstOrDefault(x => x.Type == (int)type);
            if (v != null && v.Status == (int)VerificationCheckStatus.Verified)
                score += weight;
        }

        if (await SkillTestService.HasPassingResultAsync(_context, candidateId))
            score += EvidenceSkillTestWeight;

        return score;
    }

    public async Task<int> CalculateCompatibilityIndex(Guid candidateId)
    {
        var candidateSkillIds = await _context.PT_CandidateSkills
            .Where(cs => cs.PT_CandidateId == candidateId && !cs.IsDeleted)
            .Select(cs => cs.PT_SkillId)
            .ToListAsync();

        var demandedSkillIds = await _context.PT_VacancySkills
            .Where(vs => !vs.IsDeleted && vs.Vacancy.Status == (int)VacancyStatus.Active && !vs.Vacancy.IsDeleted)
            .Select(vs => vs.PT_SkillId)
            .Distinct()
            .ToListAsync();

        // Sin vacantes activas en el sistema, neutral - no castiga al candidato por falta de
        // oferta (fase-3-sub3.md pregunta 7).
        if (demandedSkillIds.Count == 0) return 50;

        if (candidateSkillIds.Count == 0) return 0;

        var matched = candidateSkillIds.Intersect(demandedSkillIds).Count();
        var matchRatio = (double)matched / demandedSkillIds.Count;
        var baseScore = matchRatio * 100;

        // Penalizacion por skills del candidato que ninguna vacante activa demanda.
        var unmatched = candidateSkillIds.Except(demandedSkillIds).Count();
        var unusedRatio = (double)unmatched / candidateSkillIds.Count;
        var penalty = unusedRatio * UnusedSkillsMaxPenalty;

        return Math.Clamp((int)Math.Round(baseScore - penalty), 0, 100);
    }

    public int CalculateOverallScore(int stability, int reliability, int evidence, int compatibility)
    {
        var weighted = (stability * StabilityWeight) + (reliability * ReliabilityWeight) +
            (evidence * EvidenceWeight) + (compatibility * CompatibilityWeight);
        return Math.Clamp((int)Math.Round(weighted), 0, 100);
    }

    public async Task<CandidateScoreDto> RecalculateAsync(Guid candidateId)
    {
        var candidateExists = await _context.PT_Candidates.AnyAsync(c => c.Id == candidateId && !c.IsDeleted);
        if (!candidateExists) throw new InvalidOperationException("Candidate not found");

        var stability = await CalculateStabilityIndex(candidateId);
        var reliability = await CalculateReliabilityIndex(candidateId);
        var evidence = await CalculateEvidenceIndex(candidateId);
        var compatibility = await CalculateCompatibilityIndex(candidateId);
        var overall = CalculateOverallScore(stability, reliability, evidence, compatibility);

        // Se sobrescribe (upsert), sin tabla de historico separada; Version cuenta cuantas
        // veces se recalculo (fase-3-sub3.md pregunta 10, mismo patron que PTVerification).
        var score = await _context.PT_CandidateScores
            .FirstOrDefaultAsync(s => s.PT_CandidateId == candidateId && !s.IsDeleted);

        if (score == null)
        {
            score = new PTCandidateScore { PT_CandidateId = candidateId, Version = 0 };
            _context.PT_CandidateScores.Add(score);
        }

        score.StabilityIndex = stability;
        score.ReliabilityIndex = reliability;
        score.EvidenceIndex = evidence;
        score.CompatibilityIndex = compatibility;
        score.OverallScore = overall;
        score.CalculatedAt = DateTime.UtcNow;
        score.UpdatedAt = DateTime.UtcNow;
        score.Version += 1;

        await _context.SaveChangesAsync();

        return new CandidateScoreDto
        {
            CandidateId = candidateId,
            StabilityIndex = stability,
            ReliabilityIndex = reliability,
            EvidenceIndex = evidence,
            CompatibilityIndex = compatibility,
            OverallScore = overall,
            CalculatedAt = score.CalculatedAt,
            Version = score.Version
        };
    }

    public async Task<CandidateScoreDto> GetScoreAsync(Guid candidateId)
    {
        var score = await _context.PT_CandidateScores
            .FirstOrDefaultAsync(s => s.PT_CandidateId == candidateId && !s.IsDeleted);

        if (score == null)
        {
            return new CandidateScoreDto { CandidateId = candidateId, CalculatedAt = DateTime.UtcNow, Version = 0 };
        }

        return new CandidateScoreDto
        {
            CandidateId = candidateId,
            StabilityIndex = score.StabilityIndex,
            ReliabilityIndex = score.ReliabilityIndex,
            EvidenceIndex = score.EvidenceIndex,
            CompatibilityIndex = score.CompatibilityIndex,
            OverallScore = score.OverallScore,
            CalculatedAt = score.CalculatedAt,
            Version = score.Version
        };
    }

    public async Task RecalculateAllAsync()
    {
        // Bajo demanda unicamente - no hay Hangfire/Quartz instalado (fase-3-sub3.md
        // pregunta 8, mismo gap ya documentado en fase-3-sub2.md pregunta 6). Queda lista para
        // que un futuro job la invoque sin cambios.
        var candidateIds = await _context.PT_Candidates
            .Where(c => !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var id in candidateIds)
        {
            await RecalculateAsync(id);
        }
    }

    private static DateTime ResolveEnd(DateTime start, DateTime? end, bool isCurrentJob, DateTime now)
        => isCurrentJob || !end.HasValue ? now : end.Value;

    private static int MonthsBetween(DateTime start, DateTime end)
        => ((end.Year - start.Year) * 12) + (end.Month - start.Month);
}
