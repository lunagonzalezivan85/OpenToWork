using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public interface ILinkedinSearchService
{
    Task<LinkedinSearchResponseDto> SearchAsync(string? country, string? city, string? position);
}

public class LinkedinSearchService : ILinkedinSearchService
{
    private readonly string _googleApiKey;
    private readonly string _searchEngineId;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkedinSearchService> _logger;

    public LinkedinSearchService(HttpClient httpClient, IConfiguration config, ILogger<LinkedinSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _googleApiKey = config["GoogleSearch:ApiKey"] ?? "";
        _searchEngineId = config["GoogleSearch:SearchEngineId"] ?? "";
    }

    public async Task<LinkedinSearchResponseDto> SearchAsync(string? country, string? city, string? position)
    {
        try
        {
            var queryParts = new List<string> { "site:linkedin.com/in" };

            if (!string.IsNullOrWhiteSpace(position))
                queryParts.Add($"\"{position.Trim()}\"");
            if (!string.IsNullOrWhiteSpace(city))
                queryParts.Add($"\"{city.Trim()}\"");
            if (!string.IsNullOrWhiteSpace(country))
                queryParts.Add($"\"{country.Trim()}\"");

            var query = string.Join(" ", queryParts);
            var searchUrl = $"https://www.googleapis.com/customsearch/v1?key={_googleApiKey}&cx={_searchEngineId}&q={WebUtility.UrlEncode(query)}&num=10";

            _logger.LogInformation("Google Custom Search: {Url}", searchUrl);

            var response = await _httpClient.GetAsync(searchUrl);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google Custom Search error: {StatusCode} - {Response}", response.StatusCode, responseText);
                return new LinkedinSearchResponseDto
                {
                    Success = false,
                    Error = $"Google Search API returned {response.StatusCode}: {responseText}"
                };
            }

            var results = ParseGoogleCustomSearchResponse(responseText);

            _logger.LogInformation("Parsed {Count} LinkedIn profiles", results.Count);

            return new LinkedinSearchResponseDto
            {
                Success = true,
                Results = results,
                TotalResults = results.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching LinkedIn profiles");
            return new LinkedinSearchResponseDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private List<LinkedinSearchResultDto> ParseGoogleCustomSearchResponse(string responseText)
    {
        var results = new List<LinkedinSearchResultDto>();

        try
        {
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;

            if (!root.TryGetProperty("items", out var items))
            {
                _logger.LogWarning("Google Custom Search response has no 'items' property");
                return results;
            }

            foreach (var item in items.EnumerateArray())
            {
                var url = item.TryGetProperty("link", out var link) ? link.GetString() ?? "" : "";
                var title = item.TryGetProperty("title", out var titleEl) ? titleEl.GetString() : null;
                var snippet = item.TryGetProperty("snippet", out var snippetEl) ? snippetEl.GetString() : null;

                if (!url.Contains("linkedin.com/in"))
                    continue;

                var name = ExtractNameFromTitle(title) ?? ExtractNameFromUrl(url);

                results.Add(new LinkedinSearchResultDto
                {
                    Name = name,
                    Url = url,
                    Title = title,
                    Snippet = snippet
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Google Custom Search response");
        }

        return results;
    }

    private string? ExtractNameFromTitle(string? title)
    {
        if (string.IsNullOrEmpty(title)) return null;

        // LinkedIn titles are usually "Nombre Apellido - Título | LinkedIn"
        var dashIndex = title.IndexOf(" - ");
        if (dashIndex > 0)
            return title.Substring(0, dashIndex).Trim();

        var pipeIndex = title.IndexOf(" | ");
        if (pipeIndex > 0)
            return title.Substring(0, pipeIndex).Trim();

        return title.Trim();
    }

    private string ExtractNameFromUrl(string url)
    {
        var inIndex = url.IndexOf("/in/", StringComparison.OrdinalIgnoreCase);
        if (inIndex < 0) return url;

        var slug = url.Substring(inIndex + 4).TrimEnd('/');
        var decoded = Uri.UnescapeDataString(slug);
        var parts = decoded.Split('-', StringSplitOptions.RemoveEmptyEntries);

        var nameParts = parts
            .Where(p => p.Length > 1 && !p.All(char.IsDigit))
            .Take(4)
            .Select(p => char.ToUpper(p[0]) + p.Substring(1))
            .ToList();

        return nameParts.Count > 0 ? string.Join(" ", nameParts) : decoded;
    }
}
