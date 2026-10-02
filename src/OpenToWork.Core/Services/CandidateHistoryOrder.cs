using OpenToWork.Models.Entities;

namespace OpenToWork.Core.Services;

/// <summary>
/// Orden de la experiencia y la formacion del candidato: de la mas reciente a la mas antigua (pedido
/// de Darwin, 1-Oct). Lo usan todas las vistas (perfil, panel, ficha que ve la empresa, admin), para
/// que el CV se lea igual en todas partes.
/// </summary>
public static class CandidateHistoryOrder
{
    /// <summary>Trabajo actual primero; despues por fecha de inicio y de fin, descendente.</summary>
    public static IEnumerable<PTCandidateExperience> Experiences(IEnumerable<PTCandidateExperience> items) =>
        items.Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.IsCurrentJob)
            .ThenByDescending(e => e.StartDate)
            .ThenByDescending(e => e.EndDate ?? DateTime.MaxValue);

    /// <summary>En curso primero; despues por fecha de fin (o de inicio si no hay fin), descendente.</summary>
    public static IEnumerable<PTCandidateEducation> Educations(IEnumerable<PTCandidateEducation> items) =>
        items.Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.IsInProgress)
            .ThenByDescending(e => e.EndDate ?? e.StartDate ?? DateTime.MinValue)
            .ThenByDescending(e => e.StartDate ?? DateTime.MinValue);
}
