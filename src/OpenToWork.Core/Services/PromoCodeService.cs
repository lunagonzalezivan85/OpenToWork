using Microsoft.EntityFrameworkCore;
using OpenToWork.Core.Interfaces;
using OpenToWork.Models.Context;
using OpenToWork.Models.Entities;
using OpenToWork.Shared.DTOs;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Core.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly IJobPricingService _pricing;

    public PromoCodeService(AppDbContext context, IAuditLogService auditLog, IJobPricingService pricing)
    {
        _context = context;
        _auditLog = auditLog;
        _pricing = pricing;
    }

    public async Task<List<PromoCodeDto>> GetAllAsync()
    {
        return await _context.PT_PromoCodes
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromoCodeDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                PT_JobLevelId = p.PT_JobLevelId,
                JobLevelName = p.JobLevel != null ? p.JobLevel.Name : null,
                PT_JobTypeId = p.PT_JobTypeId,
                JobTypeName = p.JobType != null ? p.JobType.Name : null,
                ValidFrom = p.ValidFrom,
                ValidUntil = p.ValidUntil,
                MaxUses = p.MaxUses,
                UsesCount = p.UsesCount,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<PromoCodeDto> CreateAsync(SavePromoCodeDto dto, Guid adminId, string? ipAddress)
    {
        var code = NormalizeCode(dto.Code);
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("El codigo no puede estar vacio.");

        var exists = await _context.PT_PromoCodes.AnyAsync(p => p.Code == code && !p.IsDeleted);
        if (exists)
            throw new InvalidOperationException($"Ya existe un codigo promocional \"{code}\".");

        ValidateDiscountValue(dto.DiscountType, dto.DiscountValue);

        var promo = new PTPromoCode
        {
            Code = code,
            Description = dto.Description?.Trim(),
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            PT_JobLevelId = dto.PT_JobLevelId,
            PT_JobTypeId = dto.PT_JobTypeId,
            ValidFrom = dto.ValidFrom ?? DateTime.UtcNow,
            ValidUntil = dto.ValidUntil,
            MaxUses = dto.MaxUses,
            IsActive = dto.IsActive,
            CreatedBy = adminId
        };
        _context.PT_PromoCodes.Add(promo);
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "CreatePromoCode", "PT_PromoCodes", promo.Id, $"{{\"code\":\"{promo.Code}\"}}", ipAddress);

        return await MapToDto(promo);
    }

    public async Task<PromoCodeDto?> UpdateAsync(Guid id, SavePromoCodeDto dto, Guid adminId, string? ipAddress)
    {
        var promo = await _context.PT_PromoCodes.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (promo == null) return null;

        var code = NormalizeCode(dto.Code);
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("El codigo no puede estar vacio.");

        var exists = await _context.PT_PromoCodes.AnyAsync(p => p.Code == code && p.Id != id && !p.IsDeleted);
        if (exists)
            throw new InvalidOperationException($"Ya existe un codigo promocional \"{code}\".");

        ValidateDiscountValue(dto.DiscountType, dto.DiscountValue);

        promo.Code = code;
        promo.Description = dto.Description?.Trim();
        promo.DiscountType = dto.DiscountType;
        promo.DiscountValue = dto.DiscountValue;
        promo.PT_JobLevelId = dto.PT_JobLevelId;
        promo.PT_JobTypeId = dto.PT_JobTypeId;
        promo.ValidFrom = dto.ValidFrom ?? promo.ValidFrom;
        promo.ValidUntil = dto.ValidUntil;
        promo.MaxUses = dto.MaxUses;
        promo.IsActive = dto.IsActive;
        promo.UpdatedAt = DateTime.UtcNow;
        promo.UpdatedBy = adminId;

        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "UpdatePromoCode", "PT_PromoCodes", promo.Id, $"{{\"code\":\"{promo.Code}\"}}", ipAddress);

        return await MapToDto(promo);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId, string? ipAddress)
    {
        var promo = await _context.PT_PromoCodes.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (promo == null) return false;

        promo.IsDeleted = true;
        promo.DeletedAt = DateTime.UtcNow;
        promo.DeletedBy = adminId;
        await _context.SaveChangesAsync();
        await _auditLog.LogAsync(adminId, "DeletePromoCode", "PT_PromoCodes", promo.Id, null, ipAddress);
        return true;
    }

    public async Task<PromoValidationResultDto> ValidateAsync(string code, Guid vacancyId)
    {
        var normalized = NormalizeCode(code);
        if (string.IsNullOrWhiteSpace(normalized))
            return Invalid("Ingresa un codigo.");

        var vacancy = await _context.PT_Vacancies
            .Where(v => v.Id == vacancyId && !v.IsDeleted)
            .Select(v => new { v.PT_JobTypeId, JobLevelId = v.JobType != null ? v.JobType.PT_JobLevelId : (Guid?)null })
            .FirstOrDefaultAsync();
        if (vacancy == null)
            return Invalid("La vacante no existe.");
        if (vacancy.PT_JobTypeId == null)
            return Invalid("Esta vacante no tiene un tipo de puesto asignado; no se puede calcular el precio ni aplicar el codigo.");

        var price = await _pricing.GetActivePriceAsync(vacancy.PT_JobTypeId.Value);
        if (price == null)
            return Invalid("El tipo de puesto de esta vacante todavia no tiene un precio de lista definido.");

        var promo = await _context.PT_PromoCodes.FirstOrDefaultAsync(p => p.Code == normalized && !p.IsDeleted);
        if (promo == null) return Invalid("El codigo no existe.");
        if (!promo.IsActive) return Invalid("El codigo esta desactivado.");

        var now = DateTime.UtcNow;
        if (now < promo.ValidFrom) return Invalid("El codigo todavia no esta vigente.");
        if (promo.ValidUntil.HasValue && now > promo.ValidUntil.Value) return Invalid("El codigo vencio.");
        if (promo.MaxUses.HasValue && promo.UsesCount >= promo.MaxUses.Value) return Invalid("El codigo alcanzo su limite de usos.");

        if (promo.PT_JobTypeId.HasValue && promo.PT_JobTypeId.Value != vacancy.PT_JobTypeId.Value)
            return Invalid("Este codigo no aplica al tipo de puesto de esta vacante.");
        if (!promo.PT_JobTypeId.HasValue && promo.PT_JobLevelId.HasValue && promo.PT_JobLevelId.Value != vacancy.JobLevelId)
            return Invalid("Este codigo no aplica al nivel de puesto de esta vacante.");

        var discount = promo.DiscountType == (int)PromoDiscountType.Percentage
            ? Math.Round(price.BasePrice * promo.DiscountValue / 100m, 2)
            : promo.DiscountValue;
        discount = Math.Min(discount, price.BasePrice);

        return new PromoValidationResultDto
        {
            IsValid = true,
            PromoCodeId = promo.Id,
            Code = promo.Code,
            DiscountAmount = discount,
            FinalAmount = price.BasePrice - discount
        };
    }

    public async Task IncrementUsageAsync(Guid promoCodeId)
    {
        var promo = await _context.PT_PromoCodes.FirstOrDefaultAsync(p => p.Id == promoCodeId);
        if (promo == null) return;
        promo.UsesCount += 1;
        await _context.SaveChangesAsync();
    }

    private static void ValidateDiscountValue(int discountType, decimal discountValue)
    {
        if (discountValue <= 0)
            throw new InvalidOperationException("El valor del descuento debe ser mayor a cero.");
        if (discountType == (int)PromoDiscountType.Percentage && discountValue > 100)
            throw new InvalidOperationException("Un descuento porcentual no puede superar 100%.");
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static PromoValidationResultDto Invalid(string message) => new() { IsValid = false, ErrorMessage = message };

    private async Task<PromoCodeDto> MapToDto(PTPromoCode promo)
    {
        var level = promo.PT_JobLevelId.HasValue
            ? await _context.PT_JobLevels.Where(l => l.Id == promo.PT_JobLevelId).Select(l => l.Name).FirstOrDefaultAsync()
            : null;
        var type = promo.PT_JobTypeId.HasValue
            ? await _context.PT_JobTypes.Where(t => t.Id == promo.PT_JobTypeId).Select(t => t.Name).FirstOrDefaultAsync()
            : null;

        return new PromoCodeDto
        {
            Id = promo.Id,
            Code = promo.Code,
            Description = promo.Description,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            PT_JobLevelId = promo.PT_JobLevelId,
            JobLevelName = level,
            PT_JobTypeId = promo.PT_JobTypeId,
            JobTypeName = type,
            ValidFrom = promo.ValidFrom,
            ValidUntil = promo.ValidUntil,
            MaxUses = promo.MaxUses,
            UsesCount = promo.UsesCount,
            IsActive = promo.IsActive
        };
    }
}
