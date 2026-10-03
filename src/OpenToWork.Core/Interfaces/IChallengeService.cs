using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Interfaces;

/// <summary>Resultado de una operacion del candidato: Ok o un codigo de error estable para la interfaz.</summary>
public record ChallengeOpResult<T>(T? Value, string? Error, int StatusCode = 200)
{
    public static ChallengeOpResult<T> Ok(T value) => new(value, null);
    public static ChallengeOpResult<T> Fail(string error, int status = 400) => new(default, error, status);
}

/// <summary>"Demuestra tus habilidades": recorrido del candidato y consulta de resultados.</summary>
public interface IChallengeService
{
    Task<List<ChallengeJobTypeDto>> GetJobTypesAsync();
    Task<List<ChallengeCardDto>> GetCatalogAsync(Guid userId, Guid jobTypeId);
    Task<ChallengeOpResult<ChallengeIntroDto>> GetIntroAsync(Guid userId, Guid challengeId);
    Task<ChallengeOpResult<AttemptViewDto>> StartAttemptAsync(Guid userId, Guid challengeId, StartAttemptDto dto);
    Task<ChallengeOpResult<AttemptViewDto>> GetAttemptAsync(Guid userId, Guid attemptId);
    Task<ChallengeOpResult<SaveAnswerResultDto>> SaveAnswerAsync(Guid userId, Guid attemptId, string activityKey, ChallengeResponse response);
    Task<ChallengeOpResult<AttemptResultDto>> SubmitAsync(Guid userId, Guid attemptId);
    Task<ChallengeOpResult<AttemptResultDto>> GetResultAsync(Guid userId, Guid attemptId);
    Task<List<AttemptResultDto>> GetHistoryAsync(Guid userId);

    /// <summary>Empresa: solo candidatos que TD le ha entregado, solo evaluaciones entregadas, sin respuestas.</summary>
    Task<ChallengeOpResult<List<CompanyChallengeResultDto>>> GetResultsForCompanyAsync(Guid companyUserId, Guid candidateId);

    /// <summary>Equipo de TD: evaluaciones del candidato (las practicas son privadas).</summary>
    Task<List<AttemptResultDto>> GetResultsForAdminAsync(Guid candidateUserId);
}
