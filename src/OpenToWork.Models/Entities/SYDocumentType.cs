using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class SYDocumentType : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    public bool IsRequired { get; set; } = false;

    public int SortOrder { get; set; } = 0;

    public virtual ICollection<PTRecruitmentDocument> RecruitmentDocuments { get; set; } = new List<PTRecruitmentDocument>();
}
