namespace OpenToWork.Shared.DTOs;

/// <summary>Busqueda avanzada de la empresa por score/verificacion/skill - Fase 5.</summary>
public class CandidateSearchFilterDto
{
    public int? MinOverallScore { get; set; }
    /// <summary>OpenToWork.Shared.Enums.CandidateVerificationStatus - candidatos con este estado o superior.</summary>
    public int? MinVerificationStatus { get; set; }
    public Guid? SkillId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CandidateSearchResultDto
{
    public Guid CandidateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int OverallScore { get; set; }
    /// <summary>OpenToWork.Shared.Enums.CandidateVerificationStatus</summary>
    public int VerificationStatus { get; set; }
    public bool IsVerifiedTD { get; set; }
}

public class CandidateSearchResultPageDto
{
    public List<CandidateSearchResultDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class SkillOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
