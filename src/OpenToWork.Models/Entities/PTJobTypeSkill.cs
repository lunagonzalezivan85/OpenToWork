using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>Skill predeterminado para un tipo de puesto: al crear una vacante de ese tipo,
/// estos skills se sugieren como requisitos por defecto (editables por la empresa).</summary>
public class PTJobTypeSkill : BaseEntity
{
    [Required]
    public Guid PT_JobTypeId { get; set; }

    [ForeignKey("PT_JobTypeId")]
    public virtual PTJobType JobType { get; set; } = null!;

    [Required]
    public Guid PT_SkillId { get; set; }

    [ForeignKey("PT_SkillId")]
    public virtual PTSkill Skill { get; set; } = null!;

    public bool IsRequired { get; set; } = true;
}
