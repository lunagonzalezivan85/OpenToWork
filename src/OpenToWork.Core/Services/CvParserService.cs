using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenToWork.Core.Interfaces;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public interface ICvParserService
{
    Task<CvParseResultDto> ParseCvAsync(byte[] fileBytes, string fileName, string mimeType);
}

public class CvParserService : ICvParserService
{
    private const string ExtractionPrompt = @"Parse this CV/Resume and extract the following information as JSON.
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
  ""nationality"": """",
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
- Return ONLY the JSON object, no additional text.";

    private readonly string _geminiApiKey;
    private readonly string _geminiModel;
    private readonly HttpClient _httpClient;
    private readonly ISystemConfigService _systemConfig;
    private readonly ILogger<CvParserService> _logger;

    public CvParserService(HttpClient httpClient, IConfiguration config, ISystemConfigService systemConfig, ILogger<CvParserService> logger)
    {
        _httpClient = httpClient;
        _systemConfig = systemConfig;
        _logger = logger;
        _geminiApiKey = config["Gemini:ApiKey"] ?? "";
        _geminiModel = config["Gemini:Model"] ?? "gemini-3.5-flash";
    }

    public async Task<CvParseResultDto> ParseCvAsync(byte[] fileBytes, string fileName, string mimeType)
    {
        // Config de IA en SY_SystemConfig (admin /settings/company-profile). Si esta encendida y con
        // el feature de analisis de CV activo manda sobre el Gemini:ApiKey de appsettings (fallback).
        var ai = await _systemConfig.GetAiCredentialsAsync();
        var useConfigured = ai.Enabled && ai.CvAnalysisEnabled && !string.IsNullOrWhiteSpace(ai.ApiKey);

        var provider = useConfigured ? ai.Provider : "gemini";
        var apiKey = useConfigured ? ai.ApiKey : _geminiApiKey;
        var model = useConfigured ? ai.Model : _geminiModel;
        var baseUrl = useConfigured ? ai.BaseUrl.TrimEnd('/') : "";

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("AI API key is not configured.");
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("AI model is not configured.");

        var base64File = Convert.ToBase64String(fileBytes);

        var responseText = provider switch
        {
            "openai" or "groq" => await CallOpenAiCompatibleAsync(DefaultBaseUrl(provider, baseUrl), apiKey, model, base64File, fileName, mimeType),
            "anthropic" => await CallAnthropicAsync(DefaultBaseUrl(provider, baseUrl), apiKey, model, base64File, mimeType),
            "custom" => await CallOpenAiCompatibleAsync(baseUrl, apiKey, model, base64File, fileName, mimeType),
            _ => await CallGeminiAsync(baseUrl, apiKey, model, base64File, mimeType)
        };

        var textContent = provider switch
        {
            "openai" or "groq" or "custom" => ExtractOpenAiText(responseText),
            "anthropic" => ExtractAnthropicText(responseText),
            _ => ExtractGeminiText(responseText)
        };

        var cleanJson = CleanJsonResponse(textContent);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var result = JsonSerializer.Deserialize<CvParseResultDto>(cleanJson, options);
        return result ?? new CvParseResultDto();
    }

    private static string DefaultBaseUrl(string provider, string baseUrl)
    {
        if (!string.IsNullOrWhiteSpace(baseUrl)) return baseUrl;
        return provider switch
        {
            "openai" => "https://api.openai.com/v1",
            "groq" => "https://api.groq.com/openai/v1",
            "anthropic" => "https://api.anthropic.com",
            "custom" => throw new InvalidOperationException("AI base URL is required for a custom provider."),
            _ => "https://generativelanguage.googleapis.com"
        };
    }

    private async Task<string> CallGeminiAsync(string baseUrl, string apiKey, string model, string base64File, string mimeType)
    {
        var host = string.IsNullOrWhiteSpace(baseUrl) ? "https://generativelanguage.googleapis.com" : baseUrl;
        var url = $"{host}/v1beta/models/{model}:generateContent?key={apiKey}";

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
                            text = ExtractionPrompt
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

        return await PostWithRetryAsync(url, requestBody, c => c.Headers.Add("x-goog-api-key", apiKey));
    }

    private async Task<string> CallOpenAiCompatibleAsync(string baseUrl, string apiKey, string model, string base64File, string fileName, string mimeType)
    {
        var url = $"{baseUrl}/chat/completions";

        var requestBody = new
        {
            model,
            temperature = 0.1,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "file",
                            file = new
                            {
                                filename = fileName,
                                file_data = $"data:{mimeType};base64,{base64File}"
                            }
                        },
                        new
                        {
                            type = "text",
                            text = ExtractionPrompt
                        }
                    }
                }
            }
        };

        return await PostWithRetryAsync(url, requestBody, r => r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey));
    }

    private async Task<string> CallAnthropicAsync(string baseUrl, string apiKey, string model, string base64File, string mimeType)
    {
        var url = $"{baseUrl}/v1/messages";

        var requestBody = new
        {
            model,
            max_tokens = 8192,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "document",
                            source = new
                            {
                                type = "base64",
                                media_type = mimeType,
                                data = base64File
                            }
                        },
                        new
                        {
                            type = "text",
                            text = ExtractionPrompt
                        }
                    }
                }
            }
        };

        return await PostWithRetryAsync(url, requestBody, c =>
        {
            c.Headers.Add("x-api-key", apiKey);
            c.Headers.Add("anthropic-version", "2023-06-01");
        });
    }

    private async Task<string> PostWithRetryAsync(string url, object requestBody, Action<HttpRequestMessage> configureRequest)
    {
        var jsonContent = JsonSerializer.Serialize(requestBody);

        HttpResponseMessage? response = null;
        string responseText = "";

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            configureRequest(request);
            response = await _httpClient.SendAsync(request);
            responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                break;

            if ((int)response.StatusCode is 503 or 500 or 429 && attempt < 2)
            {
                _logger.LogWarning("AI API retry {Attempt}: {StatusCode}", attempt + 1, response.StatusCode);
                await Task.Delay(2000 * (attempt + 1));
                continue;
            }

            // El body del error puede contener datos del CV enviado al proveedor - solo se loggea el status.
            _logger.LogError("AI API error: {StatusCode}", response.StatusCode);
            throw new InvalidOperationException($"AI API returned {response.StatusCode}");
        }

        return responseText;
    }

    private static string ExtractGeminiText(string responseText)
    {
        using var doc = JsonDocument.Parse(responseText);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return string.IsNullOrEmpty(text)
            ? throw new InvalidOperationException("Gemini returned empty text")
            : text;
    }

    private static string ExtractOpenAiText(string responseText)
    {
        using var doc = JsonDocument.Parse(responseText);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrEmpty(text)
            ? throw new InvalidOperationException("Provider returned empty text")
            : text;
    }

    private static string ExtractAnthropicText(string responseText)
    {
        using var doc = JsonDocument.Parse(responseText);
        foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
        {
            if (block.GetProperty("type").GetString() == "text")
            {
                var text = block.GetProperty("text").GetString();
                if (!string.IsNullOrEmpty(text)) return text;
            }
        }

        throw new InvalidOperationException("Anthropic returned empty text");
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
