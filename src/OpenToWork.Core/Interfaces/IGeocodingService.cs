namespace OpenToWork.Core.Interfaces;

/// <summary>Geocodificacion de ubicaciones de texto libre a coordenadas (Nominatim/OpenStreetMap).</summary>
public interface IGeocodingService
{
    /// <summary>Resuelve una ubicacion textual ("Valencia", "Madrid, Espana") a lat/lng.
    /// Null si no se puede resolver o el proveedor falla (la geocodificacion nunca debe
    /// romper el guardado de una vacante).</summary>
    Task<(double Lat, double Lng)?> GeocodeAsync(string? location, CancellationToken ct = default);

    /// <summary>Backfill: geocodifica hasta <paramref name="max"/> vacantes con Location y sin
    /// intento previo (GeocodedAt = null). Respeta el limite de 1 req/s de Nominatim.
    /// Devuelve cuantas se resolvieron.</summary>
    Task<int> GeocodeMissingLocationsAsync(int max = 50, CancellationToken ct = default);
}
