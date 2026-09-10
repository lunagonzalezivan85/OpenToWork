namespace OpenToWork.Shared.DTOs;

public class AdminApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public DateTime? AvailableFromDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
