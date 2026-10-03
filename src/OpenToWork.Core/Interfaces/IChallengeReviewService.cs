using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Interfaces;

/// <summary>Revision humana de respuestas abiertas. canSeeAll: SuperAdmin; si no, solo candidatos asignados.</summary>
public interface IChallengeReviewService
{
    Task<List<ReviewQueueItemDto>> GetQueueAsync(Guid reviewerId, bool canSeeAll, bool includeCompleted);
    Task<ChallengeOpResult<ReviewDetailDto>> GetDetailAsync(Guid reviewerId, bool canSeeAll, Guid attemptId);
    Task<ChallengeOpResult<ReviewDetailDto>> SaveAsync(Guid reviewerId, bool canSeeAll, Guid attemptId, SaveReviewDto dto);
}
