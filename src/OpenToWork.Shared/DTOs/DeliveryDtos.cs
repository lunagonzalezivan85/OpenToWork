namespace OpenToWork.Shared.DTOs;

public class DeliverCandidateDto
{
    public Guid RecruitmentId { get; set; }
    public Guid VacancyId { get; set; }
    public string? AdminNote { get; set; }
}

public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateTitle { get; set; }
    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? AdminNote { get; set; }
    public string? CompanyFeedback { get; set; }
    public DateTime DeliveredAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int OverallScore { get; set; }
    public int ProfileCompletionPercentage { get; set; }
    public bool IsVerifiedTD { get; set; }
}

public class VacancyApplicantSummaryDto
{
    public Guid VacancyId { get; set; }
    public int Total { get; set; }
    public int InVerification { get; set; }
    public int Delivered { get; set; }
}

public class RespondDeliveryDto
{
    public int Status { get; set; }
    public string? Feedback { get; set; }
}
