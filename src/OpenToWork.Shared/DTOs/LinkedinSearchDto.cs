using System.Text.Json.Serialization;

namespace OpenToWork.Shared.DTOs;

public class LinkedinSearchRequestDto
{
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Position { get; set; }
}

public class LinkedinSearchResultDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Snippet { get; set; }
}

public class LinkedinSearchResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<LinkedinSearchResultDto> Results { get; set; } = new();
    public int TotalResults { get; set; }
}
