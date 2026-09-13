using System.ComponentModel.DataAnnotations;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Nivel de puesto (catalogo configurable por el admin): agrupa tipos de puesto para
/// fines de plazo de cobertura y garantia del Contrato Marco (secciones 5 y 6/10).
/// Ej: Operativo, Encargados y Tecnicos, Responsables y Cualificados. No es un enum
/// fijo - se puede agregar/renombrar/reordenar desde /pricing/job-levels.
/// </summary>
public class PTJobLevel : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Plazo objetivo de cobertura en dias habiles (seccion 5 del anexo).</summary>
    public int? ReferenceCoverageDays { get; set; }

    /// <summary>Periodo de garantia en dias naturales (seccion 6/10 del anexo).</summary>
    public int? WarrantyDays { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PTJobType> JobTypes { get; set; } = new List<PTJobType>();
}
