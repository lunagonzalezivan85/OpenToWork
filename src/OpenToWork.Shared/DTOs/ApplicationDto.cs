namespace OpenToWork.Shared.DTOs;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateTitle { get; set; }
    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public int Status { get; set; }
    public string? CoverLetter { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public DateTime? AvailableFromDate { get; set; }
    public int ApplicationSource { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProfileCompletionPercentage { get; set; }
}

public class CreateApplicationDto
{
    public Guid VacancyId { get; set; }
    public string? CoverLetter { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public DateTime? AvailableFromDate { get; set; }
}

public class UpdateApplicationStatusDto
{
    public int Status { get; set; }
}
