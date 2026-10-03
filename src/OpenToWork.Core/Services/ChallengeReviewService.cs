using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Services;

/// <summary>
/// Revision de respuestas abiertas de evaluaciones entregadas. El revisor ve la version exacta con la
/// que se hizo el intento (consignas y rubricas de entonces) y puntua cada criterio de 0 a 4.
/// Acceso: SuperAdmin ve todo; Reclutador solo intentos de candidatos que tiene asignados
/// (PT_CandidateRecruitments.AssignedToUserId). Se comprueba aqui, no solo en la interfaz.
/// </summary>
public class ChallengeReviewService : IChallengeReviewService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _audit;

    public ChallengeReviewService(AppDbContext context, IAuditLogService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<List<ReviewQueueItemDto>> GetQueueAsync(Guid reviewerId, bool canSeeAll, bool includeCompleted)
    {
        var statuses = includeCompleted
            ? new List<int> { (int)ReviewStatus.Pending, (int)ReviewStatus.InProgress, (int)ReviewStatus.Completed }
            : new List<int> { (int)ReviewStatus.Pending, (int)ReviewStatus.InProgress };

        var query = _context.PT_ChallengeAttempts.Include(a => a.Candidate).Include(a => a.Version).Include(a => a.Answers)
            .Where(a => !a.IsDeleted && a.Mode == (int)AttemptMode.Evaluation && a.Status == (int)AttemptStatus.Submitted
                        && statuses.Contains(a.ReviewStatus));
        if (!canSeeAll)
        {
            var assigned = _context.PT_CandidateRecruitments.Where(r => !r.IsDeleted && r.AssignedToUserId == reviewerId).Select(r => r.SCUserId);
            query = query.Where(a => assigned.Contains(a.Candidate.SCUserId));
        }
        var attempts = await query.OrderBy(a => a.SubmittedAt).ToListAsync();

        var userIds = attempts.Select(a => a.Candidate.SCUserId).Distinct().ToList();
        var recruiters = await _context.PT_CandidateRecruitments.Include(r => r.AssignedToUser)
            .Where(r => !r.IsDeleted && userIds.Contains(r.SCUserId) && r.AssignedToUserId != null)
            .Select(r => new { r.SCUserId, Name = r.AssignedToUser!.FullName ?? r.AssignedToUser.Email }).ToListAsync();
        var attemptIds = attempts.Select(a => a.Id).ToList();
        var reviews = await _context.PT_ChallengeReviews.Where(r => !r.IsDeleted && attemptIds.Contains(r.PT_ChallengeAttemptId)).ToListAsync();

        return attempts.Select(a =>
        {
            var def = ChallengeJson.Deserialize<ChallengeDefinition>(a.Version.ContentJson);
            var result = ChallengeResultBuilder.Compute(def, a, reviews.FirstOrDefault(r => r.PT_ChallengeAttemptId == a.Id));
            return new ReviewQueueItemDto
            {
                AttemptId = a.Id,
                CandidateUserId = a.Candidate.SCUserId,
                CandidateName = $"{a.Candidate.FirstName} {a.Candidate.LastName}".Trim(),
                ChallengeTitle = def.Title,
                VersionNumber = a.Version.VersionNumber,
                SubmittedAt = a.SubmittedAt,
                ReviewStatus = (ReviewStatus)a.ReviewStatus,
                PendingItems = result.PendingReviewItems,
                AssignedRecruiter = recruiters.FirstOrDefault(r => r.SCUserId == a.Candidate.SCUserId)?.Name
            };
        }).ToList();
    }

    public async Task<ChallengeOpResult<ReviewDetailDto>> GetDetailAsync(Guid reviewerId, bool canSeeAll, Guid attemptId)
    {
        var (attempt, error) = await AccessibleAttemptAsync(reviewerId, canSeeAll, attemptId);
        if (attempt == null) return ChallengeOpResult<ReviewDetailDto>.Fail(error!, error == "forbidden" ? 403 : 404);
        return ChallengeOpResult<ReviewDetailDto>.Ok(await BuildDetailAsync(attempt));
    }

    public async Task<ChallengeOpResult<ReviewDetailDto>> SaveAsync(Guid reviewerId, bool canSeeAll, Guid attemptId, SaveReviewDto dto)
    {
        var (attempt, error) = await AccessibleAttemptAsync(reviewerId, canSeeAll, attemptId);
        if (attempt == null) return ChallengeOpResult<ReviewDetailDto>.Fail(error!, error == "forbidden" ? 403 : 404);
        if (attempt.ReviewStatus == (int)ReviewStatus.Completed) return ChallengeOpResult<ReviewDetailDto>.Fail("alreadyCompleted", 409);

        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var required = RequiredCriteria(def, attempt);

        // Solo criterios que existen en la version del intento y para actividades respondidas.
        var clean = new List<ReviewScoreDto>();
        foreach (var s in dto.Scores ?? new())
        {
            if (!required.Contains((s.ActivityKey, s.CriterionKey))) continue;
            if (s.Score is < 0 or > 4) return ChallengeOpResult<ReviewDetailDto>.Fail("score", 400);
            clean.Add(new ReviewScoreDto
            {
                ActivityKey = s.ActivityKey, CriterionKey = s.CriterionKey, Score = s.Score,
                Comment = s.Comment?.Trim() is { Length: > 1000 } c ? c[..1000] : s.Comment?.Trim()
            });
        }
        clean = clean.GroupBy(s => (s.ActivityKey, s.CriterionKey)).Select(g => g.Last()).ToList();

        if (dto.Complete && required.Any(r => clean.All(s => (s.ActivityKey, s.CriterionKey) != r)))
            return ChallengeOpResult<ReviewDetailDto>.Fail("incomplete", 400);

        var review = await _context.PT_ChallengeReviews.FirstOrDefaultAsync(r => r.PT_ChallengeAttemptId == attempt.Id && !r.IsDeleted);
        if (review == null)
        {
            review = new PTChallengeReview { PT_ChallengeAttemptId = attempt.Id, ReviewerUserId = reviewerId, CreatedBy = reviewerId };
            _context.PT_ChallengeReviews.Add(review);
        }
        review.ScoresJson = ChallengeJson.Serialize(clean);
        var comment = dto.GeneralComment?.Trim();
        review.GeneralComment = comment is { Length: > 2000 } ? comment[..2000] : comment;
        review.ReviewerUserId = reviewerId;
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = reviewerId;

        attempt.ReviewStatus = dto.Complete ? (int)ReviewStatus.Completed : (int)ReviewStatus.InProgress;
        if (dto.Complete)
        {
            review.CompletedAt = DateTime.UtcNow;
            attempt.ResultJson = ChallengeJson.Serialize(ChallengeResultBuilder.Compute(def, attempt, review));
        }
        attempt.RowVersion++;
        attempt.UpdatedAt = DateTime.UtcNow;
        attempt.UpdatedBy = reviewerId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ChallengeOpResult<ReviewDetailDto>.Fail("conflict", 409);
        }
        await _audit.LogAsync(reviewerId, dto.Complete ? "ChallengeReviewCompleted" : "ChallengeReviewSaved", "PT_ChallengeAttempts", attempt.Id, null, null);
        return ChallengeOpResult<ReviewDetailDto>.Ok(await BuildDetailAsync(attempt));
    }

    /// <summary>Criterios obligatorios: los de actividades abiertas que el candidato respondio.</summary>
    internal static HashSet<(string, string)> RequiredCriteria(ChallengeDefinition def, PTChallengeAttempt attempt)
    {
        var set = new HashSet<(string, string)>();
        foreach (var a in def.Activities.Where(a => ChallengeRules.HasHumanPart(a.ResponseType)))
        {
            var ans = attempt.Answers.FirstOrDefault(x => x.ActivityKey == a.Key && !x.IsDeleted);
            if (ans == null || !ChallengeScoring.IsAnswered(a, ChallengeJson.Deserialize<ChallengeResponse>(ans.ResponseJson))) continue;
            foreach (var c in a.Rubric) set.Add((a.Key, c.Key));
        }
        return set;
    }

    private async Task<(PTChallengeAttempt? Attempt, string? Error)> AccessibleAttemptAsync(Guid reviewerId, bool canSeeAll, Guid attemptId)
    {
        var attempt = await _context.PT_ChallengeAttempts.Include(a => a.Candidate).Include(a => a.Version).Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId && !a.IsDeleted);
        // Solo evaluaciones entregadas: las practicas son privadas del candidato.
        if (attempt == null || attempt.Mode != (int)AttemptMode.Evaluation || attempt.Status != (int)AttemptStatus.Submitted)
            return (null, "notFound");
        if (!canSeeAll)
        {
            var assigned = await _context.PT_CandidateRecruitments.AnyAsync(r => !r.IsDeleted && r.SCUserId == attempt.Candidate.SCUserId && r.AssignedToUserId == reviewerId);
            if (!assigned) return (null, "forbidden");
        }
        return (attempt, null);
    }

    private async Task<ReviewDetailDto> BuildDetailAsync(PTChallengeAttempt attempt)
    {
        var def = ChallengeJson.Deserialize<ChallengeDefinition>(attempt.Version.ContentJson);
        var review = await _context.PT_ChallengeReviews.FirstOrDefaultAsync(r => r.PT_ChallengeAttemptId == attempt.Id && !r.IsDeleted);
        var names = await _context.PT_Competencies.Where(c => !c.IsDeleted).ToDictionaryAsync(c => c.Id, c => c.Name);
        var reviewer = review == null ? null
            : await _context.SC_Users.Where(u => u.Id == review.ReviewerUserId).Select(u => u.FullName ?? u.Email).FirstOrDefaultAsync();
        var jobTypes = await _context.PT_ChallengeJobTypes.Include(j => j.JobType)
            .Where(j => j.PT_ChallengeId == attempt.PT_ChallengeId && !j.IsDeleted).Select(j => j.JobType.Name).ToListAsync();

        var computed = ChallengeResultBuilder.Compute(def, attempt, review);
        var result = ChallengeResultBuilder.ToDto(attempt, attempt.Version, def, computed, names, jobTypes, review);
        // Para el revisor: el parcial incluye lo que ya puntuo aunque no haya completado.
        if (attempt.ReviewStatus != (int)ReviewStatus.Completed) result.CompletePercent = computed.CompletePercent;

        return new ReviewDetailDto
        {
            AttemptId = attempt.Id,
            CandidateName = $"{attempt.Candidate.FirstName} {attempt.Candidate.LastName}".Trim(),
            CandidateUserId = attempt.Candidate.SCUserId,
            ChallengeTitle = def.Title,
            VersionNumber = attempt.Version.VersionNumber,
            Mode = (AttemptMode)attempt.Mode,
            SubmittedAt = attempt.SubmittedAt,
            ReviewStatus = (ReviewStatus)attempt.ReviewStatus,
            Answers = def.Activities.Select(a =>
            {
                var ans = attempt.Answers.FirstOrDefault(x => x.ActivityKey == a.Key && !x.IsDeleted);
                return new ReviewAnswerView
                {
                    Activity = a,
                    Response = ans == null ? null : ChallengeJson.Deserialize<ChallengeResponse>(ans.ResponseJson),
                    AutoPercent = ans?.AutoScore == null ? null : Math.Round(ans.AutoScore.Value * 100, 1)
                };
            }).ToList(),
            Scores = review == null ? new() : ChallengeJson.Deserialize<List<ReviewScoreDto>>(review.ScoresJson),
            GeneralComment = review?.GeneralComment,
            ReviewerName = reviewer,
            Result = result,
            CompetencyNames = names
        };
    }
}
