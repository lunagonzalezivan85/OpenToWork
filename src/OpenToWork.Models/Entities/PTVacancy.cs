using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTVacancy : BaseEntity
{
    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public int ContractType { get; set; } = 0;

    public int WorkMode { get; set; } = 0;

    [MaxLength(100)]
    public string? Category { get; set; }

    public int? ExperienceLevel { get; set; }

    public int? EnglishLevel { get; set; }

    public int Status { get; set; } = 0;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int ViewsCount { get; set; } = 0;

    /// <summary>
    /// Pesos del Job Match Score que la empresa configuro para esta vacante (scorecard, sub-fase
    /// 3.8), JSON ej. {"skills":0.5,"experience":0.3,"location":0.2}. Null = usar los defaults
    /// de CompatibilityService. Agregado en 3.8 - en 3.4 este dato vivia solo en
    /// PTJobMatchScore.WeightsConfig (el resultado de un calculo ya hecho), sin un lugar propio
    /// de la vacante para que la empresa lo configure antes de que exista ningun match calculado.
    /// </summary>
    public string? WeightsConfig { get; set; }

    public virtual ICollection<PTApplication> Applications { get; set; } = new List<PTApplication>();
    public virtual ICollection<PTVacancySkill> VacancySkills { get; set; } = new List<PTVacancySkill>();
}
