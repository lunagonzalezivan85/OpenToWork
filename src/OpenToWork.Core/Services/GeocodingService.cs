using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;

namespace OpenToWork.Core.Services;

/// <summary>
/// Geocodificacion via Nominatim (OpenStreetMap). Politica de uso del servidor publico:
/// User-Agent identificable, maximo 1 req/s y cache de resultados. Configurable por
/// appsettings Geocoding:* (base URL, country codes, user agent) para apuntar a una
/// instancia propia si el volumen lo requiere.
/// </summary>
public class GeocodingService : IGeocodingService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly HttpClient _http;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeocodingService> _logger;
    private readonly string _baseUrl;
    private readonly string? _countryCodes;
    private readonly string _userAgent;

    public GeocodingService(HttpClient http, AppDbContext context, IMemoryCache cache,
        IConfiguration configuration, ILogger<GeocodingService> logger)
    {
        _http = http;
        _context = context;
        _cache = cache;
        _logger = logger;
        _baseUrl = configuration["Geocoding:NominatimBaseUrl"]?.TrimEnd('/')
            ?? "https://nominatim.openstreetmap.org";
        // Sin countrycodes por defecto: hay vacantes fuera de Espana (ej. Cartagena, Colombia) y
        // el texto de ubicacion ya suele llevar el pais. Para restringir: Geocoding:CountryCodes = "es".
        _countryCodes = configuration["Geocoding:CountryCodes"];
        _userAgent = configuration["Geocoding:UserAgent"]
            ?? "OpenToWork/1.0 (+https://tratodirecto.es)";
    }

    public async Task<(double Lat, double Lng)?> GeocodeAsync(string? location, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;

        var key = "geo:" + location.Trim().ToLowerInvariant();
        if (_cache.TryGetValue<(double, double)?>(key, out var cached))
            return cached;

        (double, double)? result = null;
        try
        {
            var url = $"{_baseUrl}/search?format=jsonv2&limit=1&q={Uri.EscapeDataString(location.Trim())}";
            if (!string.IsNullOrEmpty(_countryCodes))
                url += $"&countrycodes={Uri.EscapeDataString(_countryCodes)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(_userAgent);
            request.Headers.AcceptLanguage.ParseAdd("es");

            var response = await _http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
            {
                var hits = await response.Content.ReadFromJsonAsync<NominatimHit[]>(ct);
                var hit = hits?.FirstOrDefault();
                if (hit != null
                    && double.TryParse(hit.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                    && double.TryParse(hit.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                {
                    result = (lat, lng);
                }
            }
            else
            {
                _logger.LogWarning("Nominatim devolvio {Status} geocodificando una ubicacion", (int)response.StatusCode);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning("No se pudo geocodificar una ubicacion: {Message}", ex.Message);
        }

        // Tambien se cachean los fallos para no repetir peticiones por ubicaciones no resolubles.
        _cache.Set(key, result, CacheTtl);
        return result;
    }

    public async Task<int> GeocodeMissingLocationsAsync(int max = 50, CancellationToken ct = default)
    {
        var pending = await _context.PT_Vacancies
            .Where(v => !v.IsDeleted && v.Location != null && v.Location != "" && v.GeocodedAt == null)
            .OrderBy(v => v.CreatedAt)
            .Take(Math.Clamp(max, 1, 500))
            .ToListAsync(ct);

        var resolved = 0;
        var first = true;
        foreach (var vacancy in pending)
        {
            ct.ThrowIfCancellationRequested();
            // Politica Nominatim: max 1 req/s.
            if (!first) await Task.Delay(1100, ct);
            first = false;

            var coords = await GeocodeAsync(vacancy.Location, ct);
            vacancy.Latitude = coords?.Lat;
            vacancy.Longitude = coords?.Lng;
            vacancy.GeocodedAt = DateTime.UtcNow;
            if (coords.HasValue) resolved++;
        }

        if (pending.Count > 0) await _context.SaveChangesAsync(ct);
        return resolved;
    }

    private sealed class NominatimHit
    {
        [JsonPropertyName("lat")] public string? Lat { get; set; }
        [JsonPropertyName("lon")] public string? Lon { get; set; }
    }
}
