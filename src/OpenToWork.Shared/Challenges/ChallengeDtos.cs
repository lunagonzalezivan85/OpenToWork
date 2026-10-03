namespace OpenToWork.Shared.Challenges;

// Estados separados: contenido (reto), intento y revision.
public enum ChallengeStatus { Draft = 0, Published = 1, Archived = 2 }
public enum AttemptMode { Practice = 0, Evaluation = 1 }
public enum AttemptStatus { InProgress = 0, Submitted = 1 }
public enum ReviewStatus { NotRequired = 0, Pending = 1, InProgress = 2, Completed = 3 }

// ===================== Candidato =====================
// Nada de esto lleva claves, soluciones, explicaciones (en evaluacion) ni rubricas.

public class ChallengeJobTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int ChallengeCount { get; set; }
}

public class ChallengeCardDto
{
    public Guid ChallengeId { get; set; }
    public string Title { get; set; } = "";
    public string Situation { get; set; } = "";
    public List<string> Competencies { get; set; } = new();
    public ChallengeLevel Level { get; set; }
    public int EstimatedMinutes { get; set; }
    public int ActivityCount { get; set; }
    public bool AllowPractice { get; set; }
    public bool AllowEvaluation { get; set; }
    /// <summary>notStarted | inProgress | submitted | reviewed</summary>
    public string CandidateStatus { get; set; } = "notStarted";
}

public class ChallengeIntroDto
{
    public Guid ChallengeId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsArchived { get; set; }
    public string Title { get; set; } = "";
    public string Situation { get; set; } = "";
    public ChallengeLevel Level { get; set; }
    public int EstimatedMinutes { get; set; }
    public int ActivityCount { get; set; }
    public string Instructions { get; set; } = "";
    public string Deliverable { get; set; } = "";
    public string AllowedTools { get; set; } = "";
    public string AssessmentDescription { get; set; } = "";
    public List<string> Competencies { get; set; } = new();
    public bool HasHumanReview { get; set; }
    public bool AllowPractice { get; set; }
    public bool AllowEvaluation { get; set; }
    public int MaxEvaluationAttempts { get; set; }
    public int RetryCooldownDays { get; set; }
    public int EvaluationsUsed { get; set; }
    public DateTime? NextEvaluationAvailableAt { get; set; }
    /// <summary>Null si puede empezar una evaluacion; si no, el motivo (attempts | cooldown | archived).</summary>
    public string? EvaluationBlockedReason { get; set; }
    public Guid? PracticeInProgressId { get; set; }
    public Guid? EvaluationInProgressId { get; set; }
}

public class NumericFieldView
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Decimals { get; set; }
}

public class ActivityViewDto
{
    public string Key { get; set; } = "";
    public string Title { get; set; } = "";
    public string Context { get; set; } = "";
    public string Prompt { get; set; } = "";
    public ChallengeTemplate Template { get; set; }
    public ChallengeResponseType ResponseType { get; set; }
    public List<ResourceDefinition> Resources { get; set; } = new();
    public List<OptionDefinition> Options { get; set; } = new();
    public List<ConversationLine> Conversation { get; set; } = new();
    public List<NumericFieldView> NumericFields { get; set; } = new();
    public string ErrorResourceKey { get; set; } = "";
    public bool AllowMultipleSelection { get; set; }
    /// <summary>Se avisa de que las selecciones incorrectas restan (sin penalizaciones ocultas).</summary>
    public bool PartialScoringNotice { get; set; }
    public bool HasHumanReview { get; set; }
    public int MaxTextLength { get; set; }

    public static ActivityViewDto From(ActivityDefinition a)
    {
        var multipleLike = a.ResponseType is ChallengeResponseType.MultipleChoice or ChallengeResponseType.ErrorDetection
            || (a.ResponseType == ChallengeResponseType.ActionsWithJustification && a.Scoring.AllowMultipleSelection);
        return new ActivityViewDto
        {
            Key = a.Key,
            Title = a.Title,
            Context = a.Context,
            Prompt = a.Prompt,
            Template = a.Template,
            ResponseType = a.ResponseType,
            Resources = a.Resources,
            Options = a.ResponseType == ChallengeResponseType.Ordering ? ShuffledForOrdering(a) : a.Options,
            Conversation = a.Conversation,
            NumericFields = a.Scoring.NumericFields
                .Select(f => new NumericFieldView { Key = f.Key, Label = f.Label, Unit = f.Unit, Decimals = f.Decimals }).ToList(),
            ErrorResourceKey = a.ResponseType == ChallengeResponseType.ErrorDetection ? a.Scoring.ErrorResourceKey : "",
            AllowMultipleSelection = a.ResponseType is ChallengeResponseType.MultipleChoice or ChallengeResponseType.ErrorDetection
                || (a.ResponseType == ChallengeResponseType.ActionsWithJustification && a.Scoring.AllowMultipleSelection),
            PartialScoringNotice = multipleLike && a.Scoring.MultipleMode == MultipleChoiceScoring.Partial,
            HasHumanReview = ChallengeRules.HasHumanPart(a.ResponseType),
            MaxTextLength = a.MaxTextLength
        };
    }

    /// <summary>
    /// En ordenacion el orden de las opciones no puede ser la solucion (el contenido suele escribirse
    /// ya ordenado). Barajado estable (mismo orden en cada recarga) y, si aun puntua, invertido.
    /// </summary>
    private static List<OptionDefinition> ShuffledForOrdering(ActivityDefinition a)
    {
        var list = a.Options.OrderBy(o => StableHash(a.Key + "|" + o.Key)).ToList();
        if (ChallengeScoring.OrderingScore(a.Scoring, list.Select(o => o.Key).ToList()) == 1) list.Reverse();
        return list;
    }

    private static uint StableHash(string s)
    {
        var h = 2166136261u; // FNV-1a: string.GetHashCode cambia entre procesos
        foreach (var ch in s) h = (h ^ ch) * 16777619u;
        return h;
    }
}

public class PracticeFeedbackDto
{
    /// <summary>Parte automatica 0-100 (null si la actividad es solo de revision humana).</summary>
    public decimal? AutoPercent { get; set; }
    public string Explanation { get; set; } = "";
    /// <summary>La actividad tiene una parte que corregiria una persona (en practica no se revisa).</summary>
    public bool HasHumanPart { get; set; }
}

public class SavedAnswerDto
{
    public ChallengeResponse Response { get; set; } = new();
    public DateTime SavedAt { get; set; }
    /// <summary>Solo en practica.</summary>
    public PracticeFeedbackDto? Feedback { get; set; }
}

public class AttemptViewDto
{
    public Guid AttemptId { get; set; }
    public Guid ChallengeId { get; set; }
    public string Title { get; set; } = "";
    public int VersionNumber { get; set; }
    public AttemptMode Mode { get; set; }
    public AttemptStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public List<ActivityViewDto> Activities { get; set; } = new();
    public Dictionary<string, SavedAnswerDto> Answers { get; set; } = new();
}

public class SaveAnswerResultDto
{
    public bool Saved { get; set; }
    public DateTime SavedAt { get; set; }
    public PracticeFeedbackDto? Feedback { get; set; }
}

public class CompetencyResultView
{
    public string Name { get; set; } = "";
    public decimal? Percent { get; set; }
    public bool IsComplete { get; set; }
}

public class AttemptResultDto
{
    public Guid AttemptId { get; set; }
    public Guid ChallengeId { get; set; }
    public string ChallengeTitle { get; set; } = "";
    public List<string> JobTypes { get; set; } = new();
    public int VersionNumber { get; set; }
    public AttemptMode Mode { get; set; }
    public AttemptStatus Status { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int DurationSeconds { get; set; }
    public decimal? AutoPercent { get; set; }
    public decimal? CompletePercent { get; set; }
    public int PendingReviewItems { get; set; }
    public List<CompetencyResultView> Competencies { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> ToPractice { get; set; } = new();
    public string? ReviewerComment { get; set; }
}

public class StartAttemptDto
{
    public AttemptMode Mode { get; set; }
    /// <summary>Evaluacion: el candidato acepta las condiciones mostradas.</summary>
    public bool AcceptConditions { get; set; }
}

// ===================== Admin =====================

public class CompetencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int UsedInChallenges { get; set; }
}

public class SaveCompetencyDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class ChallengeJobTypeConfigDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? LevelName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public List<Guid> CompetencyIds { get; set; } = new();
    public List<string> ChallengeTitles { get; set; } = new();
}

public class SaveChallengeJobTypeDto
{
    public string? Description { get; set; }
    public List<Guid> CompetencyIds { get; set; } = new();
}

public class ChallengeListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public ChallengeStatus Status { get; set; }
    public bool HasDraftChanges { get; set; }
    public int? LatestVersion { get; set; }
    public List<string> JobTypes { get; set; } = new();
    public int ActivityCount { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ChallengeEditDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public ChallengeStatus Status { get; set; }
    public bool HasDraftChanges { get; set; }
    public int? LatestVersion { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public List<Guid> JobTypeIds { get; set; } = new();
    public ChallengeDefinition Definition { get; set; } = new();
    public bool CanDelete { get; set; }
    public int AttemptCount { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<ChallengeHistoryEntryDto> History { get; set; } = new();
}

public class ChallengeHistoryEntryDto
{
    public DateTime At { get; set; }
    public string Action { get; set; } = "";
    public string? By { get; set; }
}

public class SaveChallengeDto
{
    public List<Guid> JobTypeIds { get; set; } = new();
    public ChallengeDefinition Definition { get; set; } = new();
}

public class ChallengeActionResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public int? VersionNumber { get; set; }
    public Guid? Id { get; set; }
}

public class PreviewEvaluateDto
{
    public ActivityDefinition Activity { get; set; } = new();
    public ChallengeResponse Response { get; set; } = new();
}

public class SeedResultDto
{
    public int JobTypesCreated { get; set; }
    public int CompetenciesCreated { get; set; }
    public int ChallengesCreated { get; set; }
    public int ChallengesSkipped { get; set; }
    public int ChallengesPublished { get; set; }
    public List<string> Errors { get; set; } = new();
}

// ===================== Revision =====================

public class ReviewQueueItemDto
{
    public Guid AttemptId { get; set; }
    public Guid CandidateUserId { get; set; }
    public string CandidateName { get; set; } = "";
    public string ChallengeTitle { get; set; } = "";
    public int VersionNumber { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public int PendingItems { get; set; }
    public string? AssignedRecruiter { get; set; }
}

public class ReviewAnswerView
{
    public ActivityDefinition Activity { get; set; } = new();
    public ChallengeResponse? Response { get; set; }
    public decimal? AutoPercent { get; set; }
}

public class ReviewDetailDto
{
    public Guid AttemptId { get; set; }
    public string CandidateName { get; set; } = "";
    public Guid CandidateUserId { get; set; }
    public string ChallengeTitle { get; set; } = "";
    public int VersionNumber { get; set; }
    public AttemptMode Mode { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public List<ReviewAnswerView> Answers { get; set; } = new();
    public List<ReviewScoreDto> Scores { get; set; } = new();
    public string? GeneralComment { get; set; }
    public string? ReviewerName { get; set; }
    public AttemptResultDto Result { get; set; } = new();
    /// <summary>Nombres de competencia por Id (los de la version pueden estar desactivados hoy).</summary>
    public Dictionary<Guid, string> CompetencyNames { get; set; } = new();
}

public class ReviewScoreDto
{
    public string ActivityKey { get; set; } = "";
    public string CriterionKey { get; set; } = "";
    public int Score { get; set; }
    public string? Comment { get; set; }
}

public class SaveReviewDto
{
    public List<ReviewScoreDto> Scores { get; set; } = new();
    public string? GeneralComment { get; set; }
    /// <summary>true = completar la revision (exige todos los criterios puntuados).</summary>
    public bool Complete { get; set; }
}

// ===================== Empresa =====================

public class CompanyChallengeResultDto
{
    public string ChallengeTitle { get; set; } = "";
    public int VersionNumber { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public decimal? AutoPercent { get; set; }
    public decimal? CompletePercent { get; set; }
    public int PendingReviewItems { get; set; }
    public List<CompetencyResultView> Competencies { get; set; } = new();
}
