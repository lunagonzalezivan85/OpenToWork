namespace OpenToWork.Shared.DTOs;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int PrimaryRole { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? CandidateName { get; set; }
    public bool? WizardCompleted { get; set; }
    public bool? HasLinkedIn { get; set; }
    public bool? HasPortfolio { get; set; }
    public bool? HasCV { get; set; }
    public bool? HasScore { get; set; }
}

public class AdminVacancyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Location { get; set; }
    public int ContractType { get; set; }
    public int WorkMode { get; set; }
    public int Status { get; set; }
    public bool IsTemporary { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewsCount { get; set; }
}

public class ModerateVacancyDto
{
    public int Status { get; set; }
}

public class ChangeRoleDto
{
    public int Role { get; set; }
}

public class AdminSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class CreateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class DashboardMetricsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalPermanentVacancies { get; set; }
    public int TotalTempVacancies { get; set; }
    public Dictionary<string, int> VacanciesByStatus { get; set; } = new();
    public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();
    public int TotalSkills { get; set; }
    public int TotalAuditLogEntries { get; set; }

    public int EvaluatedProfiles { get; set; }
    public int PendingProfiles { get; set; }
    public int ProfilesWithScores { get; set; }
    public int OpenVacancies { get; set; }
    public int ClosedVacancies { get; set; }
    public int DraftVacancies { get; set; }
    public int CompaniesWithVacancies { get; set; }
    public int CompaniesWithoutVacancies { get; set; }
    public int NonAdminUsers { get; set; }
    public int NonAdminCandidates { get; set; }
    public int NonAdminCompanies { get; set; }
    public int CandidatesWithLinkedIn { get; set; }
    public int CandidatesWithPortfolio { get; set; }
    public int CandidatesWithCV { get; set; }
}

public class AdminUserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int PrimaryRole { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Id de PTCandidate (no de SCUser) - null si este usuario no es candidato. Agregado en Fase 3, sub-fase 3.8 para poder llamar los endpoints de scoring/verificaciones desde el admin.</summary>
    public Guid? CandidateId { get; set; }
    public string? CandidateName { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Phone { get; set; }
    public string? Identification { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? Gender { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? CvUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool? WizardCompleted { get; set; }
    public int? Availability { get; set; }
    public int? WorkAuthorization { get; set; }
    public bool? IsProfilePublic { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<AdminCandidateSkillDto> Skills { get; set; } = new();
    public List<AdminCandidateExperienceDto> Experiences { get; set; } = new();
    public List<AdminCandidateEducationDto> Educations { get; set; } = new();
    public List<AdminCandidateCertificationDto> Certifications { get; set; } = new();

    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? CompanyLinkedInUrl { get; set; }
    public bool? IsVerified { get; set; }
    public int VacancyCount { get; set; }
}

public class AdminCandidateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int? ProficiencyLevel { get; set; }
}

public class AdminCandidateExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentJob { get; set; }
    public string? Location { get; set; }
}

public class AdminCandidateEducationDto
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsInProgress { get; set; }
}

public class AdminCandidateCertificationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Issuer { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
}

public class CandidateConsoleDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public bool WizardCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool HasLinkedIn { get; set; }
    public bool HasPortfolio { get; set; }
    public bool HasCV { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public int? Availability { get; set; }
    public int SkillCount { get; set; }
    public int ExperienceCount { get; set; }
    public int ApplicationCount { get; set; }
    public List<string> TopSkills { get; set; } = new();
}

public class CandidateConsoleResultDto
{
    public List<CandidateConsoleDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public CandidateConsoleStatsDto Stats { get; set; } = new();
}

public class CandidateConsoleStatsDto
{
    public int TotalCandidates { get; set; }
    public int EvaluatedProfiles { get; set; }
    public int PendingProfiles { get; set; }
    public int WithLinkedIn { get; set; }
    public int WithPortfolio { get; set; }
    public int WithCV { get; set; }
    public int WithApplications { get; set; }
    public int ActiveCandidates { get; set; }
    public int InactiveCandidates { get; set; }
}
