using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Services;

public class SystemConfigService : ISystemConfigService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;

    public const string CompanyLegalName = "company_legal_name";
    public const string CompanyTaxId = "company_tax_id";
    public const string CompanyAddress = "company_address";
    public const string CompanyMercantileRegistry = "company_mercantile_registry";
    public const string LegalRepName = "legal_rep_name";
    public const string LegalRepPosition = "legal_rep_position";
    public const string LegalRepDni = "legal_rep_dni";
    public const string JurisdictionCity = "jurisdiction_city";

    public SystemConfigService(AppDbContext context, IAuditLogService auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<List<SystemConfigDto>> GetAllAsync()
    {
        return await _context.SY_SystemConfig
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Category).ThenBy(c => c.Key)
            .Select(c => new SystemConfigDto
            {
                Id = c.Id,
                Key = c.Key,
                Value = c.Value,
                Category = c.Category,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task UpdateBulkAsync(UpdateSystemConfigDto dto, Guid staffId)
    {
        var keys = dto.Items.Select(i => i.Key).ToList();
        var configs = await _context.SY_SystemConfig
            .Where(c => !c.IsDeleted && keys.Contains(c.Key))
            .ToListAsync();

        foreach (var item in dto.Items)
        {
            var config = configs.FirstOrDefault(c => c.Key == item.Key);
            if (config == null) continue;
            config.Value = item.Value;
            config.UpdatedAt = DateTime.UtcNow;
            config.UpdatedBy = staffId;
        }

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(staffId, "UpdateSystemConfig", "SY_SystemConfig", null, null, null);
    }

    public async Task<CompanyIdentityDto> GetCompanyIdentityAsync()
    {
        var values = await _context.SY_SystemConfig
            .Where(c => !c.IsDeleted && c.Category == "CompanyIdentity")
            .ToDictionaryAsync(c => c.Key, c => c.Value);

        string Get(string key, string fallback) => !string.IsNullOrWhiteSpace(values.GetValueOrDefault(key)) ? values[key]! : fallback;

        return new CompanyIdentityDto
        {
            LegalName = Get(CompanyLegalName, "TRATO DIRECTO HUMAN SERVICES, S.L."),
            TaxId = Get(CompanyTaxId, "1235567"),
            Address = Get(CompanyAddress, "Calle de Lugo 15, Alcobendas"),
            MercantileRegistry = Get(CompanyMercantileRegistry, "Registro Mercantil de Madrid, Tomo 1, Folio 1, Hoja 1"),
            LegalRepName = Get(LegalRepName, "Luis Alejandro Velasquez"),
            LegalRepPosition = Get(LegalRepPosition, "Representante Legal"),
            LegalRepDni = Get(LegalRepDni, "3212312345"),
            JurisdictionCity = Get(JurisdictionCity, "Madrid")
        };
    }
}
