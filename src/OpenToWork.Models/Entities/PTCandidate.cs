using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenToWork.Shared.Enums;

namespace OpenToWork.Models.Entities;

public class PTCandidate : BaseEntity
{
    [Required]
    public Guid SCUserId { get; set; }

    /// <summary>Nivel del plan de mejora (Free/Basic/Premium). El nivel "real" vigente se calcula con
    /// PlanCalculator.IsActive(PlanExpiresAt) - si vencio, se trata como Free aunque este campo diga otra cosa.</summary>
    public CandidatePlanTier PlanTier { get; set; } = CandidatePlanTier.Free;

    /// <summary>Fecha en que vence el plan actual. Null = sin plan de pago (Free). Hoy la fija un admin
    /// (+1 mes al asignar); cuando este Stripe, la fija el webhook con el current_period_end real.</summary>
    public DateTime? PlanExpiresAt { get; set; }

    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }

    [MaxLength(100)]
    public string? StripeSubscriptionId { get; set; }

    [ForeignKey("SCUserId")]
    public virtual SCUser User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Identification { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? Gender { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Summary { get; set; }

    [MaxLength(500)]
    public string? CvUrl { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public bool WizardCompleted { get; set; } = false;

    public int WizardStep { get; set; } = 0;

    public int? YearsOfExperience { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(500)]
    public string? PortfolioUrl { get; set; }

    public int? Availability { get; set; }

    public int? WorkAuthorization { get; set; }

    [MaxLength(50)]
    public string? WorkAuthorizations { get; set; }

    public bool? HasTransport { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }

    public bool? HasPassport { get; set; }

    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    public bool IsProfilePublic { get; set; } = true;

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<PTCandidateSkill> CandidateSkills { get; set; } = new List<PTCandidateSkill>();
    public virtual ICollection<PTCandidateExperience> Experiences { get; set; } = new List<PTCandidateExperience>();
    public virtual ICollection<PTCandidateEducation> Educations { get; set; } = new List<PTCandidateEducation>();
    public virtual ICollection<PTCandidateCertification> Certifications { get; set; } = new List<PTCandidateCertification>();
    public virtual ICollection<PTApplication> Applications { get; set; } = new List<PTApplication>();
}
