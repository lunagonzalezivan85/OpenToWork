namespace OpenToWork.Shared.DTOs;

public class CreateReferenceDto
{
    public string ContactName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Relationship { get; set; }
}

public class CandidateReferenceDto
{
    public Guid Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Relationship { get; set; }
    public int Status { get; set; }
    public int? Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTime? SentAt { get; set; }
    /// <summary>Aviso no bloqueante: otra referencia del mismo candidato ya tiene este CompanyName (fase-3-sub5.md pregunta 8).</summary>
    public bool SameCompanyAsAnotherReference { get; set; }
}

public class CandidateReferencesListDto
{
    public List<CandidateReferenceDto> References { get; set; } = new();
    /// <summary>fase-3-sub5.md pregunta 1 - informativo, no bloquea nada en este servicio.</summary>
    public bool HasMinimumReferences { get; set; }
}

/// <summary>
/// Resultado de SendReferenceRequestAsync: no hay SMTP en el proyecto (mismo gap que
/// AuthService.RequestPasswordResetAsync), asi que se devuelve el link para que el candidato
/// lo comparta manualmente (fase-3-sub5.md pregunta 2).
/// </summary>
public class ReferenceRequestLinkDto
{
    public Guid ReferenceId { get; set; }
    public string ShareableLink { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>Endpoint publico - el token identifica la referencia, no hace falta el Id ni auth.</summary>
public class SubmitReferenceFeedbackDto
{
    public string Token { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}
