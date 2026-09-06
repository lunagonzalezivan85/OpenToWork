using System.ComponentModel.DataAnnotations;

namespace OpenToWork.Models.Entities;

/// <summary>
/// Banco de retos tecnicos. Version 1: solo multiple choice (ver fase-3-sub1.md - codigo
/// ejecutable requeriria un judge/sandbox externo, mayor alcance). Ver plan obligatorio de
/// Fase 3 en README, sub-fase 3.1/3.6.
/// </summary>
public class PTSkillTest : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>OpenToWork.Shared.Enums.SkillTestDifficulty: Easy=0, Medium=1, Hard=2</summary>
    public int Difficulty { get; set; } = 0;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int TimeLimit { get; set; } = 15;

    /// <summary>JSON con las preguntas de multiple choice: [{"question":"...","options":["..."],"correctIndex":0}]</summary>
    public string? Questions { get; set; }

    public bool IsActive { get; set; } = true;
}
