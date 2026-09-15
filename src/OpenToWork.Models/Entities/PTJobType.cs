using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Tipo de puesto (catalogo configurable): la categoria real de la vacante (Camarero/a,
/// Barman, Jefe/a de sala...). Cada tipo pertenece a un nivel de puesto (PTJobLevel) para
/// plazos/garantia, pero el precio de contratacion se fija por tipo, no por nivel -
/// un Camarero y un Barman pueden ser ambos "Operativo" y costar distinto.
/// </summary>
public class PTJobType : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid PT_JobLevelId { get; set; }

    [ForeignKey("PT_JobLevelId")]
    public virtual PTJobLevel JobLevel { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PTJobTypePrice> Prices { get; set; } = new List<PTJobTypePrice>();

    public virtual ICollection<PTJobTypeSkill> DefaultSkills { get; set; } = new List<PTJobTypeSkill>();
}
