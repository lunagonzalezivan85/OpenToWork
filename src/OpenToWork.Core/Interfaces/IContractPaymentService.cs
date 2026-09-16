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

    /// <summary>Crea un cargo ad-hoc (TrancheType=ReposicionSegunda) sobre el contrato, monto =
    /// FeeAmount * percentage / 100. A diferencia de CreateTranchesForContractAsync, NO es
    /// idempotente: siempre inserta una fila nueva (se llama una sola vez, cuando se activa la
    /// 2a reposicion de garantia). Devuelve el Id del tramo creado.</summary>
    Task<Guid> CreateReplacementChargeAsync(Guid contractId, decimal percentage, string description);
}
