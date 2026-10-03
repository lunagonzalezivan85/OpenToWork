using OpenToWork.Shared.Challenges;

namespace OpenToWork.Core.Interfaces;

/// <summary>Administracion de "Retos y competencias": contenido, versiones y carga inicial.</summary>
public interface IChallengeAdminService
{
    Task<List<CompetencyDto>> GetCompetenciesAsync();
    Task<ChallengeActionResultDto> CreateCompetencyAsync(SaveCompetencyDto dto, Guid adminId);
    Task<ChallengeActionResultDto> UpdateCompetencyAsync(Guid id, SaveCompetencyDto dto, Guid adminId);

    Task<List<ChallengeJobTypeConfigDto>> GetJobTypesAsync();
    Task<ChallengeActionResultDto> SaveJobTypeAsync(Guid jobTypeId, SaveChallengeJobTypeDto dto, Guid adminId);

    Task<List<ChallengeListItemDto>> GetChallengesAsync();
    Task<ChallengeEditDto?> GetChallengeAsync(Guid id);
    Task<ChallengeActionResultDto> CreateChallengeAsync(SaveChallengeDto dto, Guid adminId);
    /// <summary>Guarda el borrador. Si el reto ya estaba publicado sigue sirviendose la ultima version
    /// hasta que se publique otra (HasDraftChanges = true).</summary>
    Task<ChallengeActionResultDto> SaveDraftAsync(Guid id, SaveChallengeDto dto, Guid adminId);
    Task<ChallengeActionResultDto> DuplicateAsync(Guid id, Guid adminId);
    /// <summary>Solo borradores nunca publicados ni usados en intentos.</summary>
    Task<ChallengeActionResultDto> DeleteAsync(Guid id, Guid adminId);
    Task<ChallengeActionResultDto> PublishAsync(Guid id, Guid adminId);
    Task<ChallengeActionResultDto> ArchiveAsync(Guid id, Guid adminId);
    Task<ChallengeActionResultDto> RestoreAsync(Guid id, Guid adminId);

    /// <summary>Vista previa: corrige una actividad sin guardar nada ni tocar perfiles.</summary>
    PracticeFeedbackDto PreviewEvaluate(PreviewEvaluateDto dto);

    /// <summary>Carga idempotente del contenido inicial (no duplica ni sobrescribe ediciones).</summary>
    Task<SeedResultDto> SeedInitialContentAsync(Guid adminId);
}
