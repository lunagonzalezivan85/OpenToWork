using OpenToWork.Models.Entities;
using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Services;

/// <summary>Calcula el resultado de un intento a partir de su version, respuestas y revision.</summary>
internal static class ChallengeResultBuilder
{
    public const decimal StrengthThreshold = 75m;
    public const decimal PracticeThreshold = 50m;

    public static ChallengeResult Compute(ChallengeDefinition def, PTChallengeAttempt attempt, PTChallengeReview? review)
    {
        var answers = attempt.Answers.Where(a => !a.IsDeleted).ToList();
        var autoScores = answers.ToDictionary(a => a.ActivityKey, a => a.AutoScore);
        var answered = answers
            .Where(a =>
            {
                var act = def.Activities.FirstOrDefault(x => x.Key == a.ActivityKey);
                return act != null && ChallengeScoring.IsAnswered(act, ChallengeJson.Deserialize<ChallengeResponse>(a.ResponseJson));
            })
            .Select(a => a.ActivityKey).ToHashSet();

        // En practica no hay revision humana: lo abierto no se puntua ni queda pendiente.
        var scores = review == null ? new List<CriterionScore>()
            : ChallengeJson.Deserialize<List<ReviewScoreDto>>(review.ScoresJson)
                .Select(s => new CriterionScore(s.ActivityKey, s.CriterionKey, s.Score)).ToList();
        var result = ChallengeScoring.Aggregate(def, autoScores, scores, answered);
        if (attempt.Mode == (int)AttemptMode.Practice)
        {
            result.PendingReviewItems = 0;
            result.CompletePercent = null;
            result.Competencies = ChallengeScoring.Aggregate(StripHuman(def), autoScores, Array.Empty<CriterionScore>(), answered).Competencies;
        }
        return result;
    }

    public static AttemptResultDto ToDto(PTChallengeAttempt attempt, PTChallengeVersion version, ChallengeDefinition def,
        ChallengeResult result, IReadOnlyDictionary<Guid, string> competencyNames, IEnumerable<string> jobTypes, PTChallengeReview? review)
    {
        var dto = new AttemptResultDto
        {
            AttemptId = attempt.Id,
            ChallengeId = attempt.PT_ChallengeId,
            ChallengeTitle = def.Title,
            JobTypes = jobTypes.ToList(),
            VersionNumber = version.VersionNumber,
            Mode = (AttemptMode)attempt.Mode,
            Status = (AttemptStatus)attempt.Status,
            ReviewStatus = (ReviewStatus)attempt.ReviewStatus,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            DurationSeconds = attempt.DurationSeconds,
            AutoPercent = result.AutoPercent,
            CompletePercent = attempt.Status == (int)AttemptStatus.Submitted ? result.CompletePercent : null,
            PendingReviewItems = result.PendingReviewItems,
            ReviewerComment = attempt.ReviewStatus == (int)ReviewStatus.Completed ? review?.GeneralComment : null
        };

        foreach (var c in result.Competencies)
        {
            dto.Competencies.Add(new CompetencyResultView
            {
                Name = competencyNames.TryGetValue(c.CompetencyId, out var n) ? n : "Competencia",
                Percent = c.Percent,
                IsComplete = c.IsComplete
            });

            // Fortalezas / a practicar: textos editoriales del reto, solo con la competencia ya corregida.
            if (!c.IsComplete || c.Percent == null) continue;
            var fb = def.CompetencyFeedback.FirstOrDefault(f => f.CompetencyId == c.CompetencyId);
            if (fb == null) continue;
            if (c.Percent >= StrengthThreshold && !string.IsNullOrWhiteSpace(fb.StrengthText)) dto.Strengths.Add(fb.StrengthText);
            if (c.Percent < PracticeThreshold && !string.IsNullOrWhiteSpace(fb.PracticeText)) dto.ToPractice.Add(fb.PracticeText);
        }
        return dto;
    }

    private static ChallengeDefinition StripHuman(ChallengeDefinition def)
    {
        var copy = ChallengeJson.Clone(def);
        foreach (var a in copy.Activities) a.Rubric.Clear();
        return copy;
    }
}
