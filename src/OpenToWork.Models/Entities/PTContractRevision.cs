using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Version anterior de un contrato de servicio, guardada al reabrirlo para corregirlo ("Abrir nueva
/// version"): el contrato vuelve a Borrador con Version + 1 y aqui queda una copia completa (JSON del
/// AdminVacancyContractDto tal como estaba) con el motivo del cambio, para revisiones futuras.
/// </summary>
public class PTContractRevision : BaseEntity
{
    [Required]
    public Guid PT_VacancyContractId { get; set; }

    [ForeignKey("PT_VacancyContractId")]
    public virtual PTVacancyContract Contract { get; set; } = null!;

    /// <summary>Numero de la version guardada (la que se reemplaza).</summary>
    public int VersionNumber { get; set; }

    /// <summary>ContractStatus que tenia esa version al reabrirse (Enviado o Aceptado).</summary>
    public int PreviousStatus { get; set; }

    /// <summary>Importe total de esa version (atajo para listar sin deserializar el snapshot).</summary>
    public decimal? FeeAmount { get; set; }

    /// <summary>Por que se abrio una nueva version (obligatorio).</summary>
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Copia completa de la version (JSON de AdminVacancyContractDto).</summary>
    [Required]
    public string SnapshotJson { get; set; } = string.Empty;
}
