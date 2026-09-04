namespace OpenToWork.Shared.DTOs;

public class CvParseResultDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Nationality { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Availability { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<CvParsedExperience> Experiences { get; set; } = new();
    public List<CvParsedEducation> Educations { get; set; } = new();
    public List<CvParsedCertification> Certifications { get; set; } = new();
    public List<CvParsedLanguage> Languages { get; set; } = new();
}

public class CvParsedExperience
{
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsCurrentJob { get; set; }
}

public class CvParsedEducation
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool IsInProgress { get; set; }
}

public class CvParsedCertification
{
    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public string? IssueDate { get; set; }
    public string? ExpiryDate { get; set; }
}

public class CvParsedLanguage
{
    public string Name { get; set; } = string.Empty;
    public string? Level { get; set; }
}

public class UploadCvResponseDto
{
    public string CvUrl { get; set; } = string.Empty;
    public CvParseResultDto ParsedData { get; set; } = new();
}

public class ApplyCvRequestDto
{
    public string CvUrl { get; set; } = string.Empty;
    public CvParseResultDto ParsedData { get; set; } = new();
}
