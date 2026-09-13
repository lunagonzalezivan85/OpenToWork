using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IContractPaymentService
{
    /// <summary>Genera los 3 tramos (Apertura/Validacion/Consolidacion) para un contrato recien
    /// aceptado, congelando % y monto segun el FeeAmount vigente. Idempotente: si ya existen
    /// tramos no-eliminados para el contrato, no hace nada.</summary>
    Task CreateTranchesForContractAsync(Guid contractId);

    Task<List<ContractPaymentDto>> GetByContractAsync(Guid contractId);

    Task<ContractPaymentDto?> MarkAsPaidAsync(Guid trancheId, MarkTranchePaidDto dto, Guid adminId);

    /// <summary>true si el contrato de la vacante dada tiene su tramo de Apertura pagado.
    /// Si la vacante no esta ligada a ningun contrato, devuelve true (no bloquea).</summary>
    Task<bool> IsOpeningPaidForVacancyAsync(Guid vacancyId);
}
