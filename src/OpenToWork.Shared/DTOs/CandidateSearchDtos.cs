namespace OpenToWork.Shared.DTOs;

/// <summary>Busqueda avanzada de la empresa por verificacion/skill - Fase 5.
/// Sin filtros por score (H-27): un "minimo" por query permite reconocer el valor
/// por biseccion - filtrar es una forma de perfilar, y el perfilado laboral espera
/// la EIPD y la base legal antes de exponerse a empresas.</summary>
public class CandidateSearchFilterDto
{
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

/// <summary>Solicitud de empresa verificada para que TD le presente a un candidato (H-39 rediseno).</summary>
public class CandidateRequestDto
{
    /// <summary>Vacante de la empresa para la que lo quieren (opcional).</summary>
    public Guid? VacancyId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Elemento de la cola del staff (AdminAPI): empresa + candidato enmascarado + vacante.</summary>
public class CandidateRequestListDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    /// <summary>SCUserId del candidato: el pipeline del admin navega por usuario, no por PTCandidate.</summary>
    public Guid CandidateUserId { get; set; }
    /// <summary>Nombre + inicial del apellido (datos minimos tambien en la cola del staff).</summary>
    public string CandidateMaskedName { get; set; } = string.Empty;
    public string? CandidateTitle { get; set; }
    public Guid? VacancyId { get; set; }
    public string? VacancyTitle { get; set; }
    public string? Notes { get; set; }
    /// <summary>0 Pendiente, 1 Gestionada, 2 Rechazada.</summary>
    public int Status { get; set; }
    public string? RequestedByEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
