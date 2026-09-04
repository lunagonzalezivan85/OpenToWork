using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTRecruitmentDocument : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    [Required]
    public Guid SY_DocumentTypeId { get; set; }

    [ForeignKey("SY_DocumentTypeId")]
    public virtual SYDocumentType DocumentType { get; set; } = null!;

    public int Status { get; set; } = 0;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(100)]
    public string? FileName { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public Guid? VerifiedByUserId { get; set; }

    [ForeignKey("VerifiedByUserId")]
    public virtual SCUser? VerifiedByUser { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
