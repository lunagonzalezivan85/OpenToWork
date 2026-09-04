using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenToWork.Models.Entities;

public class PTCandidateRecruitment : BaseEntity
{
    [Required]
    public Guid SCUserId { get; set; }

    [ForeignKey("SCUserId")]
    public virtual SCUser User { get; set; } = null!;

    public Guid? PT_VacancyId { get; set; }

    [ForeignKey("PT_VacancyId")]
    public virtual PTVacancy? Vacancy { get; set; }

    public int CurrentStage { get; set; } = 0;

    public Guid? AssignedToUserId { get; set; }

    [ForeignKey("AssignedToUserId")]
    public virtual SCUser? AssignedToUser { get; set; }

    public Guid? AssignedByUserId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? StageEnteredAt { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<PTRecruitmentStageLog> StageLogs { get; set; } = new List<PTRecruitmentStageLog>();
    public virtual ICollection<PTInvestigationChecklist> InvestigationChecklist { get; set; } = new List<PTInvestigationChecklist>();
    public virtual ICollection<PTTechnicalEvaluation> TechnicalEvaluations { get; set; } = new List<PTTechnicalEvaluation>();
    public virtual PTCandidateRecruitmentPreferences? Preferences { get; set; }
    public virtual ICollection<PTRecruitmentDocument> RecruitmentDocuments { get; set; } = new List<PTRecruitmentDocument>();
    public virtual PTRecruitmentDismissal? Dismissal { get; set; }
}
