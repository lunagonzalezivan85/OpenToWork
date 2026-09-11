using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Tabla de enlace entre contrato y vacantes (1:N). Un contrato agrupa N vacantes
/// de la misma empresa bajo un mismo anexo de servicio.
/// </summary>
public class PTContractVacancy : BaseEntity
{
    [Required]
    public Guid PT_ContractId { get; set; }

    [ForeignKey("PT_ContractId")]
    public virtual PTVacancyContract Contract { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;
}
