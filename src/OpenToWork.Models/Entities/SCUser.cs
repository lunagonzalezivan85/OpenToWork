using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class SCUser : BaseEntity
{
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public int PrimaryRole { get; set; } = 0;

    public int? StaffRole { get; set; }

    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(50)]
    public string? Identification { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Solo se usa para PrimaryRole=Admin (personal administrativo). Sin SMTP configurado en
    /// el proyecto, el vencimiento se aplica junto con el reseteo admin-mediado (ver
    /// StaffService.ResetPasswordAsync) en vez de un flujo de auto-servicio por email.
    /// </summary>
    public DateTime? PasswordExpiresAt { get; set; }

    public bool EmailVerified { get; set; } = false;

    [MaxLength(256)]
    public string? GoogleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(256)]
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetExpiresAt { get; set; }

    public virtual ICollection<SCUserRole> UserRoles { get; set; } = new List<SCUserRole>();
    public virtual ICollection<SCRefreshToken> RefreshTokens { get; set; } = new List<SCRefreshToken>();
    public virtual ICollection<SCUserDevice> UserDevices { get; set; } = new List<SCUserDevice>();
    public virtual PTCandidate? Candidate { get; set; }
    public virtual PTCompany? Company { get; set; }
    public virtual SYUserPreference? UserPreference { get; set; }
    public virtual ICollection<PTTempVacancy> TempVacancies { get; set; } = new List<PTTempVacancy>();
}
