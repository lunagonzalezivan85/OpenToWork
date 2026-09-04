using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCandidateRecruitmentPreferences : BaseEntity
{
    [Required]
    public Guid PT_CandidateRecruitmentId { get; set; }

    [ForeignKey("PT_CandidateRecruitmentId")]
    public virtual PTCandidateRecruitment Recruitment { get; set; } = null!;

    public int? PreferredWorkShift { get; set; }

    public int? AcceptedContractType { get; set; }

    public int? AvailabilityToJoin { get; set; }

    public decimal? ExpectedSalary { get; set; }

    [MaxLength(50)]
    public string? AvailableDays { get; set; }

    public bool? AvailableWeekends { get; set; }

    public bool? AvailableHolidays { get; set; }

    [MaxLength(200)]
    public string? AvailableSchedule { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }
}
