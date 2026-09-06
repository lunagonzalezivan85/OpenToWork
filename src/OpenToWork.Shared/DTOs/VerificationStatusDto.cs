namespace OpenToWork.Shared.DTOs;

/// <summary>Calculado en vivo, nunca persistido - ver fase-3-sub7.md.</summary>
public class VerificationStatusDto
{
    public Guid CandidateId { get; set; }
    /// <summary>OpenToWork.Shared.Enums.CandidateVerificationStatus</summary>
    public int Status { get; set; }
    public bool IsVerifiedTD { get; set; }
    public int ProfileCompletionPercentage { get; set; }
    public int OverallScore { get; set; }
    /// <summary>De las 4 verificaciones "gating" (LinkedIn/Portfolio/CvCoherence/Reference) - Identity excluida, es un stub (fase-3-sub7.md pregunta 2).</summary>
    public int GatingChecksRun { get; set; }
    public int GatingChecksVerified { get; set; }
    public bool HasVerifiedReference { get; set; }
}
