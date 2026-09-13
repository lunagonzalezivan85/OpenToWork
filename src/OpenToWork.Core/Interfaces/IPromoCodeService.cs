using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IPromoCodeService
{
    Task<List<PromoCodeDto>> GetAllAsync();
    Task<PromoCodeDto> CreateAsync(SavePromoCodeDto dto, Guid adminId, string? ipAddress);
    Task<PromoCodeDto?> UpdateAsync(Guid id, SavePromoCodeDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteAsync(Guid id, Guid adminId, string? ipAddress);

    /// <summary>Valida un codigo contra una vacante concreta (preview, no consume uso ni escribe redemption).</summary>
    Task<PromoValidationResultDto> ValidateAsync(string code, Guid vacancyId);

    /// <summary>Suma 1 al contador de usos. Se llama una vez por linea de contrato donde el codigo se aplico de verdad.</summary>
    Task IncrementUsageAsync(Guid promoCodeId);
}
