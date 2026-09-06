namespace OpenToWork.Shared.DTOs;

/// <summary>Resultado de una verificacion individual, refleja una fila de PT_Verifications.</summary>
public class VerificationResultDto
{
    /// <summary>OpenToWork.Shared.Enums.VerificationType</summary>
    public int Type { get; set; }

    /// <summary>OpenToWork.Shared.Enums.VerificationCheckStatus</summary>
    public int Status { get; set; }

    public int Score { get; set; }

    /// <summary>JSON con el detalle (issues encontrados, red flags, motivo del fallo, etc).</summary>
    public string? Result { get; set; }

    public DateTime? VerifiedAt { get; set; }
}
