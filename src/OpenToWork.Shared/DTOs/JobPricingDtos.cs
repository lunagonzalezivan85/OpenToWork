namespace OpenToWork.Shared.DTOs;

// ===== Niveles de puesto =====

public class JobLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ReferenceCoverageDays { get; set; }
    public int? WarrantyDays { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int JobTypeCount { get; set; }
}

public class SaveJobLevelDto
{
    public string Name { get; set; } = string.Empty;
    public int? ReferenceCoverageDays { get; set; }
    public int? WarrantyDays { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ===== Tipos de puesto =====

public class JobTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid PT_JobLevelId { get; set; }
    public string JobLevelName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? PriceEffectiveFrom { get; set; }

    /// <summary>Referencia de garantia (dias naturales) del nivel al que pertenece este tipo de
    /// puesto (PTJobLevel.WarrantyDays) - default sugerido al fijar la garantia de una vacante.</summary>
    public int? LevelWarrantyDays { get; set; }
}

public class SaveJobTypeDto
{
    public string Name { get; set; } = string.Empty;
    public Guid PT_JobLevelId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ===== Lista de precios (versionada) =====

public class JobTypePriceDto
{
    public Guid Id { get; set; }
    public Guid PT_JobTypeId { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}

public class SetJobTypePriceDto
{
    public decimal BasePrice { get; set; }
    public string? Currency { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public string? Notes { get; set; }
}

// ===== Skills predeterminados por tipo de puesto =====

public class SetJobTypeSkillsDto
{
    public List<Guid> SkillIds { get; set; } = new();
}

/// <summary>Opcion de tipo de puesto para el selector de la empresa al crear una vacante,
/// con sus skills predeterminados ya resueltos.</summary>
public class JobTypeOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobLevelName { get; set; } = string.Empty;
    public List<AdminSkillDto> DefaultSkills { get; set; } = new();
}
