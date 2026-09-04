namespace OpenToWork.Shared.DTOs;

public class CandidateRecruitmentPreferencesDto
{
    public Guid Id { get; set; }
    public Guid RecruitmentId { get; set; }
    public int? PreferredWorkShift { get; set; }
    public int? AcceptedContractType { get; set; }
    public int? AvailabilityToJoin { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? AvailableDays { get; set; }
    public bool? AvailableWeekends { get; set; }
    public bool? AvailableHolidays { get; set; }
    public string? AvailableSchedule { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class UpdateRecruitmentPreferencesDto
{
    public int? PreferredWorkShift { get; set; }
    public int? AcceptedContractType { get; set; }
    public int? AvailabilityToJoin { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? AvailableDays { get; set; }
    public bool? AvailableWeekends { get; set; }
    public bool? AvailableHolidays { get; set; }
    public string? AvailableSchedule { get; set; }
}
