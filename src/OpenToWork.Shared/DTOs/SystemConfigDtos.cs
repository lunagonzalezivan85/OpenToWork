namespace OpenToWork.Shared.DTOs;

public class SystemConfigDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SystemConfigItemDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}

public class UpdateSystemConfigDto
{
    public List<SystemConfigItemDto> Items { get; set; } = new();
}

/// <summary>Datos de identidad legal de Trato Directo usados en el Contrato Marco
/// (VacancyContractDocument.razor). Antes fijos en el Razor, ahora vienen de SY_SystemConfig.</summary>
public class CompanyIdentityDto
{
    public string LegalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string MercantileRegistry { get; set; } = string.Empty;
    public string LegalRepName { get; set; } = string.Empty;
    public string LegalRepPosition { get; set; } = string.Empty;
    public string LegalRepDni { get; set; } = string.Empty;
    public string JurisdictionCity { get; set; } = string.Empty;
    /// <summary>Correo para ejercer derechos de proteccion de datos (politica de privacidad del portal).</summary>
    public string PrivacyEmail { get; set; } = string.Empty;
}

/// <summary>Datos legales de Trato Directo que se pueden publicar (responsable del tratamiento en la politica
/// de privacidad). NO incluye el DNI del representante legal, que es un dato personal.</summary>
public class PublicLegalIdentityDto
{
    public string LegalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string MercantileRegistry { get; set; } = string.Empty;
    public string PrivacyEmail { get; set; } = string.Empty;
}
