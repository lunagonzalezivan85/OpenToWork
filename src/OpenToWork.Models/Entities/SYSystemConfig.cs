using System.ComponentModel.DataAnnotations;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Configuracion del sistema centralizada en BD (Key/Value), en vez de constantes
/// hardcodeadas en el codigo. Primer uso: los datos de identidad legal de Trato Directo
/// que aparecen en el Contrato Marco (VacancyContractDocument.razor) - antes fijos en el
/// Razor, ahora editables desde /settings/company-profile sin necesitar un deploy.
/// </summary>
public class SYSystemConfig : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = "General";

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
