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

/// <summary>Configuracion del proveedor de IA (SY_SystemConfig, categoria "Ai"). ApiKey es write-only:
/// nunca se devuelve al cliente (HasApiKey indica si hay una guardada). Los toggles por feature deciden
/// en que puntos del sistema se consume la IA; todos apagados por defecto.</summary>
public class AiSettingsDto
{
    public string Provider { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
    public bool Enabled { get; set; }
    public bool CvAnalysisEnabled { get; set; }
    public bool CommandBarEnabled { get; set; }
    public bool AdminSuggestionsEnabled { get; set; }
    public bool MatchingEnabled { get; set; }
}

/// <summary>Configuracion > Videos de candidatos: interruptor y espacio que ocupan los videos guardados.</summary>
public class PresentationVideoSettingsDto
{
    public bool Enabled { get; set; }
    public int VideoCount { get; set; }
    public long TotalBytes { get; set; }
}
