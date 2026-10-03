using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Services;

/// <summary>
/// Recorrido del candidato en "Demuestra tus habilidades". Reglas clave:
/// - El catalogo sale solo de versiones publicadas (nunca del borrador).
/// - Cada intento queda atado a su version; editar o archivar el reto no lo cambia.
/// - La correccion es del servidor; en evaluacion no se devuelven claves, soluciones ni explicaciones.
/// - Una respuesta por actividad e intento (upsert); entregar dos veces no duplica nada.
/// </summary>
public class ChallengeService : IChallengeService
{
    private readonly AppDbContext _context;

    public ChallengeService(AppDbContext context)
    {
        _context = context;
    }

    // ---------------- Catalogo ----------------

    public async Task<List<ChallengeJobTypeDto>> GetJobTypesAsync()
    {
        var links = await PublishedLinksQuery().ToListAsync();
        return links.GroupBy(l => l.JobType)
            .OrderBy(g => g.Key.JobLevel.SortOrder).ThenBy(g => g.Key.SortOrder)
            .Select(g => new ChallengeJobTypeDto
            {
                Id = g.Key.Id, Name = g.Key.Name, Description = g.Key.Description, ChallengeCount = g.Count()
            }).ToList();
    }

    public async Task<List<ChallengeCardDto>> GetCatalogAsync(Guid userId, Guid jobTypeId)
    {
        var links = await PublishedLinksQuery().Where(l => l.PT_JobTypeId == jobTypeId).OrderBy(l => l.SortOrder).ToListAsync();
        if (links.Count == 0) return new();

        var candidateId = await CandidateIdAsync(userId);
        var challengeIds = links.Select(l => l.PT_ChallengeId).ToList();
        var versionIds = links.Select(l => l.Challenge.LatestVersionId!.Value).ToList();
        var versions = await _context.PT_ChallengeVersions.Where(v => versionIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id);
        var attempts = candidateId == null ? new List<PTChallengeAttempt>()
            : await _context.PT_ChallengeAttempts.Where(a => a.PT_CandidateId == candidateId && challengeIds.Contains(a.PT_ChallengeId) && !a.IsDeleted).ToListAsync();
        var names = await CompetencyNamesAsync();

        var cards = new List<ChallengeCardDto>();
        foreach (var l in links)
        {
            var def = ChallengeJson.Deserialize<ChallengeDefinition>(versions[l.Challenge.LatestVersionId!.Value].ContentJson);
            cards.Add(new ChallengeCardDto
            {
                ChallengeId = l.PT_ChallengeId,
                Title = def.Title,
                Situation = def.Situation,
                Competencies = CompetenciesOf(def).Select(id => names.GetValueOrDefault(id, "")).Where(n => n != "").ToList(),
                Level = def.Level,
                EstimatedMinutes = def.EstimatedMinutes,
                ActivityCount = def.Activities.Count,
                AllowPractice = def.AllowPractice,
                AllowEvaluation = def.AllowEvaluation,
                CandidateStatus = StatusFor(attempts.Where(a => a.PT_ChallengeId == l.PT_ChallengeId).ToList())
            });
        }
        return cards;
    }

    /// <summary>notStarted | inProgress | submitted | reviewed (las evaluaciones mandan sobre la practica).</summary>
    internal static string StatusFor(List<PTChallengeAttempt> attempts)
    {
        var evals = attempts.Where(a => a.Mode == (int)AttemptMode.Evaluation).OrderByDescending(a => a.StartedAt).ToList();
        if (evals.Any(a => a.Status == (int)AttemptStatus.InProgress)) return "inProgress";
        var lastSubmitted = evals.FirstOrDefault(a => a.Status == (int)AttemptStatus.Submitted);
        if (lastSubmitted != null)
            return lastSubmitted.ReviewStatus == (int)ReviewStatus.Completed ? "reviewed" : "submitted";
        return attempts.Any(a => a.Status == (int)AttemptStatus.InProgress) ? "inProgress" : "notStarted";
    }

    public async Task<ChallengeOpResult<ChallengeIntroDto>> GetIntroAsync(Guid userId, Guid challengeId)
    {
        var challenge = await _context.PT_Challenges.FirstOrDefaultAsync(c => c.Id == challengeId && !c.IsDeleted);
        if (challenge?.LatestVersionId == null || challenge.Status == (int)ChallengeStatus.Draft)
            return ChallengeOpResult<ChallengeIntroDto>.Fail("notFound", 404);

        var version = await _context.PT_ChallengeVersions.FirstAsync(v => v.Id == challenge.LatestVersionId);
        var def = ChallengeJson.Deserialize<ChallengeDefinition>(version.ContentJson);
        var names = await CompetencyNamesAsync();
        var candidateId = await CandidateIdAsync(userId);
        var attempts = candidateId == null ? new List<PTChallengeAttempt>()
            : await _context.PT_ChallengeAttempts.Where(a => a.PT_CandidateId == candidateId && a.PT_ChallengeId == challengeId && !a.IsDeleted).ToListAsync();

        var (blocked, used, next) = EvaluationPolicy(def, challenge, attempts);
        return ChallengeOpResult<ChallengeIntroDto>.Ok(new ChallengeIntroDto
        {
            ChallengeId = challenge.Id,
            VersionNumber = version.VersionNumber,
            IsArchived = challenge.Status == (int)ChallengeStatus.Archived,
            Title = def.Title,
            Situation = def.Situation,
            Level = def.Level,
            EstimatedMinutes = def.EstimatedMinutes,
            ActivityCount = def.Activities.Count,
            Instructions = def.Instructions,
            Deliverable = def.Deliverable,
            AllowedTools = def.AllowedTools,
            AssessmentDescription = def.AssessmentDescription,
            Competencies = CompetenciesOf(def).Select(id => names.GetValueOrDefault(id, "")).Where(n => n != "").ToList(),
            HasHumanReview = def.Activities.Any(a => ChallengeRules.HasHumanPart(a.ResponseType)),
            AllowPractice = def.AllowPractice,
            AllowEvaluation = def.AllowEvaluation,
            MaxEvaluationAttempts = def.MaxEvaluationAttempts,
            RetryCooldownDays = def.RetryCooldownDays,
            EvaluationsUsed = used,
            NextEvaluationAvailableAt = next,
            EvaluationBlockedReason = blocked,
            PracticeInProgressId = attempts.FirstOrDefault(a => a.Mode == (int)AttemptMode.Practice && a.Status == (int)AttemptStatus.InProgress)?.Id,
            EvaluationInProgressId = attempts.FirstOrDefault(a => a.Mode == (int)AttemptMode.Evaluation && a.Status == (int)AttemptStatus.InProgress)?.Id
        });
    }

    /// <summary>Politica de reintentos de evaluacion: (motivo de bloqueo, usadas, proxima fecha).</summary>
    internal static (string? Blocked, int Used, DateTime? Next) EvaluationPolicy(ChallengeDefinition def, PTChallenge challenge, List<PTChallengeAttempt> attempts)
    {
        var submitted = attempts.Where(a => a.Mode == (int)AttemptMode.Evaluation && a.Status == (int)AttemptStatus.Submitted).ToList();
        var used = submitted.Count;
        DateTime? next = null;
        var last = submitted.OrderByDescending(a => a.SubmittedAt).FirstOrDefault();
        if (last?.SubmittedAt != null && def.RetryCooldownDays > 0)
        {
            var available = last.SubmittedAt.Value.AddDays(def.RetryCooldownDays);
            if (available > DateTime.UtcNow) next = available;
        }

        string? blocked = null;
        if (challenge.Status == (int)ChallengeStatus.Archived) blocked = "archived";
        else if (!def.AllowEvaluation) blocked = "notAllowed";
        else if (used >= def.MaxEvaluationAttempts) blocked = "attempts";
        else if (next != null) blocked = "cooldown";
        return (blocked, used, next);
    }

    // ---------------- Intentos ----------------

    public async Task<ChallengeOpResult<AttemptViewDto>> StartAttemptAsync(Guid userId, Guid challengeId, StartAttemptDto dto)
    {
        var candidateId = await CandidateIdAsync(userId);
        if (candidateId == null) return ChallengeOpResult<AttemptViewDto>.Fail("noCandidate", 403);

        var challenge = await _context.PT_Challenges.FirstOrDefaultAsync(c => c.Id == challengeId && !c.IsDeleted);
        if (challenge?.LatestVersionId == null || challenge.Status == (int)ChallengeStatus.Draft)
            return ChallengeOpResult<AttemptViewDto>.Fail("notFound", 404);

        var attempts = await _context.PT_ChallengeAttempts
            .Where(a => a.PT_CandidateId == candidateId && a.PT_ChallengeId == challengeId && !a.IsDeleted).ToListAsync();

        // Reanudar: si ya hay uno en curso de esa modalidad se devuelve el mismo (aunque el reto
        // este archivado o tenga una version nueva: sigue con la suya).
        var inProgress = attempts.FirstOrDefault(a => a.Mode == (int)dto.Mode && a.Status == (int)AttemptStatus.InProgress);
        if (inProgress != null) return await GetAttemptAsync(userId, inProgress.Id);

        if (challenge.Status == (int)ChallengeStatus.Archived) return ChallengeOpResult<AttemptViewDto>.Fail("archived", 409);

        var version = await _context.PT_ChallengeVersions.FirstAsync(v => v.Id == challenge.LatestVersionId);
        var def = ChallengeJson.Deserialize<ChallengeDefinition>(version.ContentJson);

        if (dto.Mode == AttemptMode.Practice && !def.AllowPractice) return ChallengeOpResult<AttemptViewDto>.Fail("notAllowed", 409);
        if (dto.Mode == AttemptMode.Evaluation)
        {
            var (blocked, _, _) = EvaluationPolicy(def, challenge, attempts);
            if (blocked != null) return ChallengeOpResult<AttemptViewDto>.Fail(blocked, 409);
            if (!dto.AcceptConditions) return ChallengeOpResult<AttemptViewDto>.Fail("conditions", 400);
        }

        var attempt = new PTChallengeAttempt
        {
            PT_CandidateId = candidateId.Value,
            PT_ChallengeId = challenge.Id,
            PT_ChallengeVersionId = version.Id,
            Mode = (int)dto.Mode,
            Status = (int)AttemptStatus.InProgress,
            ReviewStatus = (int)ReviewStatus.NotRequired,
            StartedAt = DateTime.UtcNow,
            ConditionsAcceptedAt = dto.Mode == AttemptMode.Evaluation ? DateTime.UtcNow : null,
            CreatedBy = userId
        };
        _context.PT_ChallengeAttempts.Add(attempt);
        await _context.SaveChangesAsync();
        return await GetAttemptAsync(userId, attempt.Id);
    }

    public async Task<ChallengeOpResult<AttemptViewDto>> GetAttemptAsync(Guid userId, Guid attemptId)
    {
        var (attempt, error) = await OwnAttemptAsync(userId, attemptId);
        if (attempt == null) return ChallengeOpResult<AttemptViewDto>.Fail(error!, error == "forbidden" ? 403 : 404);

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var practice = attempt.Mode == (int)AttemptMode.Practice;
        var view = new AttemptViewDto
        {
            AttemptId = attempt.Id,
            ChallengeId = attempt.PT_ChallengeId,
            Title = def.Title,
            VersionNumber = attempt.Version.VersionNumber,
            Mode = (AttemptMode)attempt.Mode,
            Status = (AttemptStatus)attempt.Status,
            StartedAt = attempt.StartedAt,
            Activities = def.Activities.Select(ActivityViewDto.From).ToList()
        };
        foreach (var ans in attempt.Answers.Where(a => !a.IsDeleted))
        {
            var act = def.Activities.FirstOrDefault(a => a.Key == ans.ActivityKey);
            if (act == null) continue;
            view.Answers[ans.ActivityKey] = new SavedAnswerDto
            {
                Response = ChallengeJson.Deserialize<ChallengeResponse>(ans.ResponseJson),
                SavedAt = ans.SavedAt,
                Feedback = practice ? Feedback(act, ans.AutoScore) : null
            };
        }
        return ChallengeOpResult<AttemptViewDto>.Ok(view);
    }

    public async Task<ChallengeOpResult<SaveAnswerResultDto>> SaveAnswerAsync(Guid userId, Guid attemptId, string activityKey, ChallengeResponse response)
    {
        var (attempt, error) = await OwnAttemptAsync(userId, attemptId);
        if (attempt == null) return ChallengeOpResult<SaveAnswerResultDto>.Fail(error!, error == "forbidden" ? 403 : 404);
        if (attempt.Status != (int)AttemptStatus.InProgress) return ChallengeOpResult<SaveAnswerResultDto>.Fail("locked", 409);

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var act = def.Activities.FirstOrDefault(a => a.Key == activityKey);
        if (act == null) return ChallengeOpResult<SaveAnswerResultDto>.Fail("activity", 404);

        // Nunca se confia en el cliente: se limpia contra la definicion y se corrige aqui.
        var clean = ChallengeScoring.Sanitize(act, response ?? new ChallengeResponse());
        var auto = ChallengeScoring.ScoreAuto(act, clean);
        var now = DateTime.UtcNow;

        var existing = attempt.Answers.FirstOrDefault(a => a.ActivityKey == activityKey && !a.IsDeleted);
        if (existing == null)
        {
            existing = new PTChallengeAnswer
            {
                PT_ChallengeAttemptId = attempt.Id, ActivityKey = activityKey, ResponseJson = ChallengeJson.Serialize(clean),
                AutoScore = auto, SavedAt = now, CreatedBy = userId
            };
            _context.PT_ChallengeAnswers.Add(existing);
        }
        else
        {
            existing.ResponseJson = ChallengeJson.Serialize(clean);
            existing.AutoScore = auto;
            existing.SavedAt = now;
            existing.SaveCount++;
            existing.UpdatedAt = now;
            existing.UpdatedBy = userId;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dos guardados simultaneos de la misma actividad: el indice unico evita el duplicado;
            // se reintenta como actualizacion de la fila que gano.
            _context.ChangeTracker.Clear();
            var row = await _context.PT_ChallengeAnswers.FirstOrDefaultAsync(a => a.PT_ChallengeAttemptId == attempt.Id && a.ActivityKey == activityKey);
            if (row == null) return ChallengeOpResult<SaveAnswerResultDto>.Fail("saveFailed", 500);
            row.ResponseJson = ChallengeJson.Serialize(clean);
            row.AutoScore = auto;
            row.SavedAt = now;
            row.SaveCount++;
            await _context.SaveChangesAsync();
        }

        return ChallengeOpResult<SaveAnswerResultDto>.Ok(new SaveAnswerResultDto
        {
            Saved = true,
            SavedAt = now,
            Feedback = attempt.Mode == (int)AttemptMode.Practice ? Feedback(act, auto) : null
        });
    }

    public async Task<ChallengeOpResult<AttemptResultDto>> SubmitAsync(Guid userId, Guid attemptId)
    {
        var (attempt, error) = await OwnAttemptAsync(userId, attemptId);
        if (attempt == null) return ChallengeOpResult<AttemptResultDto>.Fail(error!, error == "forbidden" ? 403 : 404);

        // Idempotente: entregar de nuevo devuelve el mismo resultado sin cambiar nada.
        if (attempt.Status == (int)AttemptStatus.Submitted) return await GetResultAsync(userId, attemptId);

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var now = DateTime.UtcNow;
        var answeredOpen = attempt.Answers.Any(a =>
        {
            var act = def.Activities.FirstOrDefault(x => x.Key == a.ActivityKey);
            return act != null && ChallengeRules.HasHumanPart(act.ResponseType)
                && !string.IsNullOrWhiteSpace(ChallengeJson.Deserialize<ChallengeResponse>(a.ResponseJson).Text);
        });

        attempt.Status = (int)AttemptStatus.Submitted;
        attempt.SubmittedAt = now;
        attempt.DurationSeconds = (int)Math.Min(int.MaxValue, Math.Max(0, (now - attempt.StartedAt).TotalSeconds));
        // Las practicas no se revisan nunca: son privadas y no son evidencia.
        attempt.ReviewStatus = attempt.Mode == (int)AttemptMode.Evaluation && answeredOpen
            ? (int)ReviewStatus.Pending : (int)ReviewStatus.NotRequired;
        attempt.ResultJson = ChallengeJson.Serialize(ChallengeResultBuilder.Compute(def, attempt, null));
        attempt.RowVersion++;
        attempt.UpdatedAt = now;
        attempt.UpdatedBy = userId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Otro "Entregar" gano la carrera: se devuelve lo que quedo guardado.
            _context.ChangeTracker.Clear();
        }
        return await GetResultAsync(userId, attemptId);
    }

    public async Task<ChallengeOpResult<AttemptResultDto>> GetResultAsync(Guid userId, Guid attemptId)
    {
        var (attempt, error) = await OwnAttemptAsync(userId, attemptId);
        if (attempt == null) return ChallengeOpResult<AttemptResultDto>.Fail(error!, error == "forbidden" ? 403 : 404);
        if (attempt.Status != (int)AttemptStatus.Submitted) return ChallengeOpResult<AttemptResultDto>.Fail("notSubmitted", 409);
        return ChallengeOpResult<AttemptResultDto>.Ok(await BuildResultAsync(attempt));
    }

    public async Task<List<AttemptResultDto>> GetHistoryAsync(Guid userId)
    {
        var candidateId = await CandidateIdAsync(userId);
        if (candidateId == null) return new();
        var attempts = await AttemptsQuery().Where(a => a.PT_CandidateId == candidateId && a.Status == (int)AttemptStatus.Submitted)
            .OrderByDescending(a => a.SubmittedAt).ToListAsync();
        var list = new List<AttemptResultDto>();
        foreach (var a in attempts) list.Add(await BuildResultAsync(a));
        return list;
    }

    // ---------------- Empresa y equipo ----------------

    public async Task<ChallengeOpResult<List<CompanyChallengeResultDto>>> GetResultsForCompanyAsync(Guid companyUserId, Guid candidateId)
    {
        // Misma regla que el CV: solo candidatos que TD ha entregado a esta empresa.
        var delivered = await _context.PT_CandidateDeliveries.AnyAsync(d => d.PT_CandidateId == candidateId && !d.IsDeleted
            && d.Company.SCUserId == companyUserId && !d.Company.IsDeleted);
        if (!delivered) return ChallengeOpResult<List<CompanyChallengeResultDto>>.Fail("forbidden", 403);

        var attempts = await AttemptsQuery()
            .Where(a => a.PT_CandidateId == candidateId && a.Mode == (int)AttemptMode.Evaluation && a.Status == (int)AttemptStatus.Submitted)
            .OrderByDescending(a => a.SubmittedAt).ToListAsync();
        var list = new List<CompanyChallengeResultDto>();
        foreach (var a in attempts)
        {
            var r = await BuildResultAsync(a);
            // Sin respuestas ni comentarios del revisor: no hay un permiso que lo autorice.
            list.Add(new CompanyChallengeResultDto
            {
                ChallengeTitle = r.ChallengeTitle, VersionNumber = r.VersionNumber, SubmittedAt = r.SubmittedAt,
                ReviewStatus = r.ReviewStatus, AutoPercent = r.AutoPercent, CompletePercent = r.CompletePercent,
                PendingReviewItems = r.PendingReviewItems, Competencies = r.Competencies
            });
        }
        return ChallengeOpResult<List<CompanyChallengeResultDto>>.Ok(list);
    }

    public async Task<List<AttemptResultDto>> GetResultsForAdminAsync(Guid candidateUserId)
    {
        var candidateId = await CandidateIdAsync(candidateUserId);
        if (candidateId == null) return new();
        var attempts = await AttemptsQuery()
            .Where(a => a.PT_CandidateId == candidateId && a.Mode == (int)AttemptMode.Evaluation && a.Status == (int)AttemptStatus.Submitted)
            .OrderByDescending(a => a.SubmittedAt).ToListAsync();
        var list = new List<AttemptResultDto>();
        foreach (var a in attempts) list.Add(await BuildResultAsync(a));
        return list;
    }

    // ---------------- Auxiliares ----------------

    private IQueryable<PTChallengeJobType> PublishedLinksQuery() =>
        _context.PT_ChallengeJobTypes
            .Include(l => l.Challenge)
            .Include(l => l.JobType).ThenInclude(t => t.JobLevel)
            .Where(l => !l.IsDeleted && !l.Challenge.IsDeleted && l.Challenge.Status == (int)ChallengeStatus.Published
                        && l.Challenge.LatestVersionId != null && !l.JobType.IsDeleted && l.JobType.IsActive);

    private IQueryable<PTChallengeAttempt> AttemptsQuery() =>
        _context.PT_ChallengeAttempts.Include(a => a.Version).Include(a => a.Answers).Where(a => !a.IsDeleted);

    private async Task<Guid?> CandidateIdAsync(Guid userId) =>
        await _context.PT_Candidates.Where(c => c.SCUserId == userId && !c.IsDeleted).Select(c => (Guid?)c.Id).FirstOrDefaultAsync();

    /// <summary>El intento solo es accesible para su candidato (comprobado en servidor).</summary>
    private async Task<(PTChallengeAttempt? Attempt, string? Error)> OwnAttemptAsync(Guid userId, Guid attemptId)
    {
        var attempt = await AttemptsQuery().FirstOrDefaultAsync(a => a.Id == attemptId);
        if (attempt == null) return (null, "notFound");
        var candidateId = await CandidateIdAsync(userId);
        if (candidateId == null || attempt.PT_CandidateId != candidateId) return (null, "forbidden");
        return (attempt, null);
    }

    private async Task<AttemptResultDto> BuildResultAsync(PTChallengeAttempt attempt)
    {
        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var review = await _context.PT_ChallengeReviews.FirstOrDefaultAsync(r => r.PT_ChallengeAttemptId == attempt.Id && !r.IsDeleted);
        var result = ChallengeResultBuilder.Compute(def, attempt, attempt.ReviewStatus == (int)ReviewStatus.Completed ? review : null);
        var jobTypes = await _context.PT_ChallengeJobTypes.Include(j => j.JobType)
            .Where(j => j.PT_ChallengeId == attempt.PT_ChallengeId && !j.IsDeleted).OrderBy(j => j.SortOrder)
            .Select(j => j.JobType.Name).ToListAsync();
        return ChallengeResultBuilder.ToDto(attempt, attempt.Version, def, result, await CompetencyNamesAsync(), jobTypes, review);
    }

    private async Task<Dictionary<Guid, string>> CompetencyNamesAsync() =>
        await _context.PT_Competencies.Where(c => !c.IsDeleted).ToDictionaryAsync(c => c.Id, c => c.Name);

    private static IEnumerable<Guid> CompetenciesOf(ChallengeDefinition d) => ChallengeAdminService.UsedCompetencies(d);

    private static PracticeFeedbackDto Feedback(ActivityDefinition act, decimal? auto) => new()
    {
        AutoPercent = auto == null ? null : Math.Round(auto.Value * 100, 1),
        Explanation = act.PracticeExplanation,
        HasHumanPart = ChallengeRules.HasHumanPart(act.ResponseType)
    };
}
