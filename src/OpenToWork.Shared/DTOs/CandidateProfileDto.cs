namespace OpenToWork.Shared.DTOs;

public class UpdateCandidateProfileDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public int? Availability { get; set; }
    public int? WorkAuthorization { get; set; }
    public bool? IsProfilePublic { get; set; }
    public string? CvUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Phone { get; set; }
    public string? Identification { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

public class CandidateProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? Gender { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? CvUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public int? Availability { get; set; }
    public int? WorkAuthorization { get; set; }
    public bool IsProfilePublic { get; set; }
    public List<CandidateExperienceDto> Experiences { get; set; } = new();
    public List<CandidateEducationDto> Educations { get; set; } = new();
    public List<CandidateCertificationDto> Certifications { get; set; } = new();
    public List<CandidateSkillDto> Skills { get; set; } = new();
}

public class CandidateSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int? ProficiencyLevel { get; set; }
}
