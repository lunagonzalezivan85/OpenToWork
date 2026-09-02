using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

/// <summary>
/// Busqueda avanzada de candidatos por score/verificacion/skill para la empresa - Fase 5
/// (Portal Corporativo), item que quedaba pendiente de Fase 3.
/// </summary>
public interface ICandidateSearchService
{
    Task<CandidateSearchResultPageDto> SearchAsync(CandidateSearchFilterDto filter);

    /// <summary>Skills que aparecen en al menos un candidato con perfil publico - para el filtro de busqueda.</summary>
    Task<List<SkillOptionDto>> GetSearchableSkillsAsync();
}
