namespace OpenToWork.Core.Interfaces;

public interface IWarrantyLookupService
{
    /// <summary>Dias de garantia del contrato vigente de la vacante (via PT_ContractVacancies),
    /// o null si la vacante no tiene contrato activo o el contrato no define garantia.</summary>
    Task<int?> GetWarrantyDaysForVacancyAsync(Guid vacancyId);
}
