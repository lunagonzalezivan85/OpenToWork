namespace OpenToWork.Shared.DTOs;

public class AdminRegisterCandidateManualDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? LinkedInUrl { get; set; }
    public int? YearsOfExperience { get; set; }
}

public class AdminRegisterCandidateResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Guid UserId { get; set; }
    public Guid CandidateId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailWasGenerated { get; set; }
    public string? FullName { get; set; }
    public string? CvUrl { get; set; }
    public CvParseResultDto? ParsedData { get; set; }
}
