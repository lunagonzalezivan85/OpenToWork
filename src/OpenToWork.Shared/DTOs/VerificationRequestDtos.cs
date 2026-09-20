namespace OpenToWork.Shared.DTOs;

/// <summary>
/// Documento adjunto de la solicitud de verificacion (imagen/PDF en base64).
/// Slot: "front" (anverso), "back" (reverso), "extra1", "extra2" (pasaporte / certificado UE / TIE).
/// </summary>
public class VerificationDocumentDto
{
    public string Slot { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string DataBase64 { get; set; } = "";
}

/// <summary>
/// Payload del wizard /verification-request.
/// AdminSituation: 0=Espanola, 1=Comunitario UE/EEE/Suiza, 2=Extracomunitario con TIE, 3=Sin permiso.
/// </summary>
public class SubmitVerificationRequestDto
{
    public bool HasSpanishNationality { get; set; }
    public int AdminSituation { get; set; }
    public string? FullName { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? DocumentExpiry { get; set; }
    public string? Nationality { get; set; }
    public int? PermitType { get; set; }
    public string? Phone { get; set; }
    public int? ContactTimePreference { get; set; }
    public List<VerificationDocumentDto> Documents { get; set; } = new();
}

/// <summary>Resultado de la solicitud creada (o la ultima existente).</summary>
public class VerificationRequestResultDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = "";
    public int Status { get; set; }
    public int AdminSituation { get; set; }
    public DateTime CreatedAt { get; set; }
}
