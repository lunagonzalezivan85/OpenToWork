using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public interface ICvParserService
{
    Task<CvParseResultDto> ParseCvAsync(byte[] fileBytes, string fileName, string mimeType);
}

public class CvParserService : ICvParserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<CvParserService> _logger;

    public CvParserService(HttpClient httpClient, IConfiguration config, ILogger<CvParserService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<CvParseResultDto> ParseCvAsync(byte[] fileBytes, string fileName, string mimeType)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Gemini API key is not configured. Set 'Gemini:ApiKey' in appsettings or environment variable.");

        var base64File = Convert.ToBase64String(fileBytes);

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mimeType,
                                data = base64File
                            }
                        },
                        new
                        {
                            text = @"Parse this CV/Resume and extract the following information as JSON.
Return ONLY valid JSON with this exact structure (no markdown, no code fences):
{
  ""firstName"": """",
  ""lastName"": """",
  ""email"": """",
  ""phone"": """",
  ""title"": """",
  ""summary"": """",
  ""linkedInUrl"": """",
  ""portfolioUrl"": """",
  ""city"": """",
  ""country"": """",
  ""yearsOfExperience"": null,
  ""availability"": """",
  ""skills"": [""skill1"", ""skill2""],
  ""experiences"": [
    {
      ""jobTitle"": """",
      ""companyName"": """",
      ""location"": """",
      ""description"": """",
      ""startDate"": ""YYYY-MM"",
      ""endDate"": ""YYYY-MM"",
      ""isCurrentJob"": false
    }
  ],
  ""educations"": [
    {
      ""institution"": """",
      ""degree"": """",
      ""fieldOfStudy"": """",
      ""startDate"": ""YYYY-MM"",
      ""endDate"": ""YYYY-MM"",
      ""isInProgress"": false
    }
  ],
  ""certifications"": [
    {
      ""name"": """",
      ""issuer"": """",
      ""issueDate"": ""YYYY-MM"",
      ""expiryDate"": ""YYYY-MM""
    }
  ],
  ""languages"": [
    {
      ""name"": """",
      ""level"": ""basic|intermediate|advanced|native""
    }
  ]
}

Rules:
- Use null for fields not found in the CV.
- Dates should be in YYYY-MM format. Use null if not found.
- isCurrentJob: true if the person still works there.
- isInProgress: true if education is still in progress.
- Extract as many skills as possible from the CV.
- For languages, map level to: basic, intermediate, advanced, or native.
- For availability, extract the candidate's availability status. Use: ""inmediata"", ""dos semanas"", ""un mes"", or ""no disponible"" (or English equivalents).
- Return ONLY the JSON object, no additional text."
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 8192,
                responseMimeType = "application/json"
            }
        };

        var model = _config["Gemini:Model"] ?? "gemini-3.6-flash";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseText);
            throw new InvalidOperationException($"Gemini API returned {response.StatusCode}");
        }

        return ParseGeminiResponse(responseText);
    }

    private CvParseResultDto ParseGeminiResponse(string responseText)
    {
        using var doc = JsonDocument.Parse(responseText);
        var root = doc.RootElement;

        var textContent = root
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(textContent))
            throw new InvalidOperationException("Gemini returned empty text");

        var cleanJson = CleanJsonResponse(textContent);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var result = JsonSerializer.Deserialize<CvParseResultDto>(cleanJson, options);
        return result ?? new CvParseResultDto();
    }

    private static string CleanJsonResponse(string text)
    {
        var cleaned = text.Trim();

        if (cleaned.StartsWith("```json"))
            cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned[3..];

        if (cleaned.EndsWith("```"))
            cleaned = cleaned[..^3];

        return cleaned.Trim();
    }
}
