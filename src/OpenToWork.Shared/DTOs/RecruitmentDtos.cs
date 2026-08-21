namespace OpenToWork.Shared.DTOs;

public class RecruitmentPipelineDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int CurrentStage { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StageEnteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public int InvestigationCompleted { get; set; }
    public int InvestigationTotal { get; set; }
    public DismissalInfoDto? Dismissal { get; set; }
}

public class RecruitmentDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public int CurrentStage { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StageEnteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public List<StageLogDto> StageLogs { get; set; } = new();
    public List<InvestigationChecklistDto> InvestigationChecklist { get; set; } = new();
    public List<CandidateExperienceDto> WorkExperiences { get; set; } = new();
    public List<CandidateCertificationDto> Certifications { get; set; } = new();
    public List<CandidateEducationDto> Educations { get; set; } = new();
    public List<TechnicalEvaluationDto> TechnicalEvaluations { get; set; } = new();
    public DismissalInfoDto? Dismissal { get; set; }
}

public class StageLogDto
{
    public int FromStage { get; set; }
    public int ToStage { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}

public class InvestigationChecklistDto
{
    public Guid Id { get; set; }
    public int Step { get; set; }
    public string? Label { get; set; }
    public bool IsCustom { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByName { get; set; }
    public string? Notes { get; set; }
    public string? EvidenceUrl { get; set; }
    public List<ReferenceCheckDto> ReferenceChecks { get; set; } = new();
}

public class ReferenceCheckDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public int Status { get; set; }
    public DateTime? CalledAt { get; set; }
    public string? Notes { get; set; }
}

public class AddReferenceDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

public class UpdateReferenceStatusDto
{
    public int Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCandidatePhoneDto
{
    public string? Phone { get; set; }
}

public class UpdateChecklistNotesDto
{
    public string? Notes { get; set; }
}

public class TechnicalEvaluationDto
{
    public Guid Id { get; set; }
    public string EvaluationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Score { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public string? EvaluatedByName { get; set; }
    public int Type { get; set; }
    public int Recommendation { get; set; }
}

public class AddTechnicalEvaluationDto
{
    public string EvaluationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Score { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
    public int Type { get; set; } = 0;
    public int Recommendation { get; set; } = 0;
}

public class UpdateTechnicalEvaluationDto
{
    public string? EvaluationName { get; set; }
    public string? Description { get; set; }
    public decimal? Score { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Notes { get; set; }
    public int? Recommendation { get; set; }
}

public class AddCustomValidationDto
{
    public string Label { get; set; } = string.Empty;
}

public class DismissalInfoDto
{
    public int Reason { get; set; }
    public string? Notes { get; set; }
    public string DismissedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AssignCandidateDto
{
    public Guid UserId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid? VacancyId { get; set; }
    public string? Notes { get; set; }
}

public class MoveStageDto
{
    public int ToStage { get; set; }
    public string? Notes { get; set; }
}

public class ToggleInvestigationStepDto
{
    public int Step { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public string? EvidenceUrl { get; set; }
}

public class DismissCandidateDto
{
    public int Reason { get; set; }
    public string? Notes { get; set; }
}

public class RecruitmentPipelineResultDto
{
    public List<RecruitmentPipelineDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public Dictionary<int, int> CountByStage { get; set; } = new();
}
