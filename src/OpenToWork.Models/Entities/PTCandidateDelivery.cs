using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCandidateDelivery : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    [Required]
    public Guid PT_CandidateId { get; set; }

    [ForeignKey("PT_CandidateId")]
    public virtual PTCandidate Candidate { get; set; } = null!;

    [Required]
    public Guid PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy Vacancy { get; set; } = null!;

    [Required]
    public Guid PT_CompanyId { get; set; }

    [ForeignKey("PT_CompanyId")]
    public virtual PTCompany Company { get; set; } = null!;

    [Required]
    public Guid DeliveredByUserId { get; set; }

    [ForeignKey("DeliveredByUserId")]
    public virtual SCUser DeliveredByUser { get; set; } = null!;

    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    public int Status { get; set; } = (int)Shared.Enums.DeliveryStatus.Delivered;

    [MaxLength(1000)]
    public string? AdminNote { get; set; }

    [MaxLength(1000)]
    public string? CompanyFeedback { get; set; }

    public DateTime? ViewedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}
