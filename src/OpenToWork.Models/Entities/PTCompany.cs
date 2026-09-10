using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCompany : BaseEntity
{
    // Nullable: una empresa registrada por un usuario tiene su Id aqui;
    // una empresa "prospecto" captada desde el CRM administrativo no tiene
    // cuenta de usuario asociada todavia y queda en null.
    public Guid? SCUserId { get; set; }

    [ForeignKey("SCUserId")]
    public virtual SCUser? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(20)]
    public string? TaxId { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    public int? CompanySize { get; set; }

    [MaxLength(256)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? ContactPosition { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    public bool IsVerified { get; set; } = false;

    public int Status { get; set; } = (int)OpenToWork.Shared.Enums.CompanyStatus.Prospecto;

    public virtual ICollection<PTVacancy> Vacancies { get; set; } = new List<PTVacancy>();
    public virtual ICollection<PTCompanyPipeline> Pipelines { get; set; } = new List<PTCompanyPipeline>();
}
