namespace OpenToWork.Shared.DTOs;

public class CandidateDto
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
    public string? LinkedInUrl { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool WizardCompleted { get; set; }
    public int WizardStep { get; set; }
    public List<CandidateExperienceDto> Experiences { get; set; } = new();
    public List<CandidateEducationDto> Educations { get; set; } = new();
}

public class UpdateCandidateWizardDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? Gender { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int WizardStep { get; set; }
    public bool WizardCompleted { get; set; }
}
