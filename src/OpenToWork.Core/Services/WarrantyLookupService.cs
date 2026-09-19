using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;

namespace OpenToWork.Core.Services;

public class WarrantyLookupService : IWarrantyLookupService
{
    private readonly AppDbContext _context;

    public WarrantyLookupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetWarrantyDaysForVacancyAsync(Guid vacancyId)
    {
        // Garantia por vacante, no por contrato: un contrato puede agrupar vacantes de niveles
        // distintos, cada una con su propio periodo (ver PTContractVacancy.WarrantyDays).
        return await _context.PT_ContractVacancies
            .Where(cv => cv.PT_VacancyId == vacancyId && !cv.IsDeleted)
            .Select(cv => cv.WarrantyDays)
            .FirstOrDefaultAsync();
    }
}
