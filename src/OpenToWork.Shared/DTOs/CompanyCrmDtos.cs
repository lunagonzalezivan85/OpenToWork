using OpenToWork.Shared.Enums;

namespace OpenToWork.Shared.DTOs;

public class CompanyListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int Status { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public int? CurrentStage { get; set; }
    public int VacancyCount { get; set; }
}

public class CompanyDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? ContactDniNie { get; set; }
    public string? LinkedInUrl { get; set; }
    public int Status { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? SCUserId { get; set; }
    public CompanyPipelineDto? Pipeline { get; set; }
    public CompanyPlanTier PlanTier { get; set; }
    public DateTime? PlanExpiresAt { get; set; }
    public bool PlanIsActive { get; set; }
}

public class CompanyPipelineDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? City { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int CurrentStage { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StageEnteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsDismissed { get; set; }
    public string? DismissalReason { get; set; }
}

/// <summary>DTO público de empresa para la sección "Empresas que confían en nosotros" del portal.</summary>
public class PublicCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>Detalle público de empresa para su página de perfil en el portal.</summary>
public class PublicCompanyDetailDto : PublicCompanyDto
{
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsVerified { get; set; }
    public int ActiveVacancies { get; set; }
}

/// <summary>Perfil de la empresa del usuario autenticado (portal empresa).</summary>
public class MyCompanyProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>Datos editables del perfil de empresa (sin Status/IsFeatured/IsVerified — los gestiona el admin).</summary>
public class UpdateMyCompanyProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? LinkedInUrl { get; set; }
}

public class CompanyPipelineResultDto
{
    public List<CompanyPipelineDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public Dictionary<int, int> CountByStage { get; set; } = new();
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? ContactDniNie { get; set; }
    public string? LinkedInUrl { get; set; }
}

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? ContactDniNie { get; set; }
    public string? LinkedInUrl { get; set; }
    public int Status { get; set; }
}

public class AssignCompanyDto
{
    public Guid CompanyId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public string? Notes { get; set; }
}

public class CompanyMoveStageDto
{
    public int ToStage { get; set; }
    public string? Notes { get; set; }
    public DateTime? MeetingDate { get; set; }
}

public class CompanyStageLogDto
{
    public int FromStage { get; set; }
    public int ToStage { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}

public class CompanyPipelineDetailDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int CompanySize { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPosition { get; set; }
    public string? ContactDniNie { get; set; }
    public string? LinkedInUrl { get; set; }
    public int Status { get; set; }
    public int CurrentStage { get; set; }
    public string? AssignedToName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StageEnteredAt { get; set; }
    public DateTime? MeetingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsDismissed { get; set; }
    public string? DismissalReason { get; set; }
    public DateTime? DismissedAt { get; set; }
    public string? DismissedByName { get; set; }
    public List<CompanyStageLogDto> StageLogs { get; set; } = new();
}

public class DismissCompanyDto
{
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePipelineNotesDto
{
    public string? Notes { get; set; }
}

public class PlanDto
{
    public Guid Id { get; set; }
    public PlanAudience Audience { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EUR";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? Features { get; set; }
}

public class SavePlanDto
{
    public PlanAudience Audience { get; set; } = PlanAudience.Company;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EUR";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public string? Features { get; set; }
}
