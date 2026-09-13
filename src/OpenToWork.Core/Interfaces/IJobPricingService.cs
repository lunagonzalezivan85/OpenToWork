using OpenToWork.Shared.DTOs;

namespace OpenToWork.Core.Interfaces;

public interface IJobPricingService
{
    // Niveles de puesto
    Task<List<JobLevelDto>> GetJobLevelsAsync();
    Task<JobLevelDto> CreateJobLevelAsync(SaveJobLevelDto dto, Guid adminId, string? ipAddress);
    Task<JobLevelDto?> UpdateJobLevelAsync(Guid id, SaveJobLevelDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteJobLevelAsync(Guid id, Guid adminId, string? ipAddress);

    // Tipos de puesto
    Task<List<JobTypeDto>> GetJobTypesAsync();
    Task<JobTypeDto> CreateJobTypeAsync(SaveJobTypeDto dto, Guid adminId, string? ipAddress);
    Task<JobTypeDto?> UpdateJobTypeAsync(Guid id, SaveJobTypeDto dto, Guid adminId, string? ipAddress);
    Task<bool> DeleteJobTypeAsync(Guid id, Guid adminId, string? ipAddress);

    // Lista de precios (versionada)
    Task<List<JobTypePriceDto>> GetPriceHistoryAsync(Guid jobTypeId);

    /// <summary>Cierra el precio vigente (si existe) y crea uno nuevo a partir de EffectiveFrom.</summary>
    Task<JobTypePriceDto> SetPriceAsync(Guid jobTypeId, SetJobTypePriceDto dto, Guid adminId, string? ipAddress);

    /// <summary>Precio vigente de un tipo de puesto a una fecha dada (default: ahora). Null si nunca se le fijo precio.</summary>
    Task<JobTypePriceDto?> GetActivePriceAsync(Guid jobTypeId, DateTime? atDate = null);
}
