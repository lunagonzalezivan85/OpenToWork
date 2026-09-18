using Microsoft.EntityFrameworkCore;
using OpenToWork.Models.Entities;

namespace OpenToWork.Models.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SCUser> SC_Users => Set<SCUser>();
    public DbSet<SCUserRole> SC_UserRoles => Set<SCUserRole>();
    public DbSet<SCRefreshToken> SC_RefreshTokens => Set<SCRefreshToken>();
    public DbSet<SCUserDevice> SC_UserDevices => Set<SCUserDevice>();
    public DbSet<PTCandidate> PT_Candidates => Set<PTCandidate>();
    public DbSet<PTCompany> PT_Companies => Set<PTCompany>();
    public DbSet<PTTempVacancy> PT_TempVacancies => Set<PTTempVacancy>();
    public DbSet<PTSkill> PT_Skills => Set<PTSkill>();
    public DbSet<PTCandidateSkill> PT_CandidateSkills => Set<PTCandidateSkill>();
    public DbSet<SYWizardStep> SY_WizardSteps => Set<SYWizardStep>();
    public DbSet<SYUserPreference> SY_UserPreferences => Set<SYUserPreference>();
    public DbSet<PTVacancy> PT_Vacancies => Set<PTVacancy>();
    public DbSet<PTApplication> PT_Applications => Set<PTApplication>();
    public DbSet<PTCandidateExperience> PT_CandidateExperiences => Set<PTCandidateExperience>();
    public DbSet<PTCandidateEducation> PT_CandidateEducations => Set<PTCandidateEducation>();
    public DbSet<PTCandidateCertification> PT_CandidateCertifications => Set<PTCandidateCertification>();
    public DbSet<PTVacancySkill> PT_VacancySkills => Set<PTVacancySkill>();
    public DbSet<ADAuditLog> AD_AuditLogs => Set<ADAuditLog>();
    public DbSet<PTCandidateRecruitment> PT_CandidateRecruitments => Set<PTCandidateRecruitment>();
    public DbSet<PTRecruitmentStageLog> PT_RecruitmentStageLogs => Set<PTRecruitmentStageLog>();
    public DbSet<PTInvestigationChecklist> PT_InvestigationChecklists => Set<PTInvestigationChecklist>();
    public DbSet<PTReferenceCheck> PT_ReferenceChecks => Set<PTReferenceCheck>();
    public DbSet<PTTechnicalEvaluation> PT_TechnicalEvaluations => Set<PTTechnicalEvaluation>();
    public DbSet<PTRecruitmentDismissal> PT_RecruitmentDismissals => Set<PTRecruitmentDismissal>();
    public DbSet<PTCandidateRecruitmentPreferences> PT_CandidateRecruitmentPreferences => Set<PTCandidateRecruitmentPreferences>();
    public DbSet<SYDocumentType> SY_DocumentTypes => Set<SYDocumentType>();
    public DbSet<PTRecruitmentDocument> PT_RecruitmentDocuments => Set<PTRecruitmentDocument>();
    public DbSet<PTCandidateScore> PT_CandidateScores => Set<PTCandidateScore>();
    public DbSet<PTJobMatchScore> PT_JobMatchScores => Set<PTJobMatchScore>();
    public DbSet<PTVerification> PT_Verifications => Set<PTVerification>();
    public DbSet<PTCandidateReference> PT_CandidateReferences => Set<PTCandidateReference>();
    public DbSet<PTSkillTest> PT_SkillTests => Set<PTSkillTest>();
    public DbSet<PTCandidateTestResult> PT_CandidateTestResults => Set<PTCandidateTestResult>();
    public DbSet<PTNegotiation> PT_Negotiations => Set<PTNegotiation>();
    public DbSet<PTNegotiationCandidate> PT_NegotiationCandidates => Set<PTNegotiationCandidate>();
    public DbSet<PTCompanyPipeline> PT_CompanyPipelines => Set<PTCompanyPipeline>();
    public DbSet<PTCompanyStageLog> PT_CompanyStageLogs => Set<PTCompanyStageLog>();
    public DbSet<PTPlan> PT_Plans => Set<PTPlan>();
    public DbSet<PTCandidateDelivery> PT_CandidateDeliveries => Set<PTCandidateDelivery>();
    public DbSet<PTVacancyContract> PT_VacancyContracts => Set<PTVacancyContract>();
    public DbSet<PTContractVacancy> PT_ContractVacancies => Set<PTContractVacancy>();
    public DbSet<PTContractPayment> PT_ContractPayments => Set<PTContractPayment>();
    public DbSet<PTWarrantyReplacement> PT_WarrantyReplacements => Set<PTWarrantyReplacement>();
    public DbSet<SYSystemConfig> SY_SystemConfig => Set<SYSystemConfig>();
    public DbSet<PTJobLevel> PT_JobLevels => Set<PTJobLevel>();
    public DbSet<PTJobType> PT_JobTypes => Set<PTJobType>();
    public DbSet<PTJobTypePrice> PT_JobTypePrices => Set<PTJobTypePrice>();
    public DbSet<PTJobTypeSkill> PT_JobTypeSkills => Set<PTJobTypeSkill>();
    public DbSet<PTPromoCode> PT_PromoCodes => Set<PTPromoCode>();
    public DbSet<PTPromoCodeRedemption> PT_PromoCodeRedemptions => Set<PTPromoCodeRedemption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SCUser>(e =>
        {
            e.ToTable("SC_Users");
            e.HasIndex(u => u.Email).IsUnique().HasFilter("IsDeleted = 0");
            e.HasIndex(u => u.GoogleId).IsUnique().HasFilter("GoogleId IS NOT NULL AND IsDeleted = 0");
            e.HasIndex(u => new { u.IsActive, u.IsDeleted });
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PrimaryRole).HasDefaultValue(0);
            e.Property(u => u.EmailVerified).HasDefaultValue(false);
            e.Property(u => u.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SCUserRole>(e =>
        {
            e.ToTable("SC_UserRoles");
            e.HasIndex(r => new { r.SCUserId, r.Role, r.IsDeleted }).IsUnique();
            e.HasIndex(r => r.Role);
            e.Property(r => r.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        modelBuilder.Entity<SCRefreshToken>(e =>
        {
            e.ToTable("SC_RefreshTokens");
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasIndex(t => new { t.SCUserId, t.IsRevoked, t.IsDeleted });
        });

        modelBuilder.Entity<SCUserDevice>(e =>
        {
            e.ToTable("SC_UserDevices");
            e.HasIndex(d => new { d.SCUserId, d.DeviceHash, d.IsDeleted }).IsUnique();
            e.HasIndex(d => new { d.SCUserId, d.IsTrusted });
        });

        modelBuilder.Entity<PTCandidate>(e =>
        {
            e.ToTable("PT_Candidates");
            e.HasIndex(c => new { c.SCUserId, c.IsDeleted }).IsUnique();
            e.HasIndex(c => c.Identification);
            e.HasIndex(c => new { c.WizardCompleted, c.IsDeleted });
            e.Property(c => c.WizardCompleted).HasDefaultValue(false);
            e.Property(c => c.WizardStep).HasDefaultValue(0);
        });

        modelBuilder.Entity<PTCompany>(e =>
        {
            e.ToTable("PT_Companies");
            e.HasIndex(c => new { c.SCUserId, c.IsDeleted }).IsUnique();
            e.HasIndex(c => new { c.Name, c.IsDeleted });
        });

        modelBuilder.Entity<PTTempVacancy>(e =>
        {
            e.ToTable("PT_TempVacancies");
            e.HasIndex(v => new { v.SCUserId, v.IsDeleted });
            e.HasIndex(v => new { v.ExpiresAt, v.IsDeleted });
            e.HasIndex(v => new { v.IsPublished, v.IsDeleted });
        });

        modelBuilder.Entity<PTSkill>(e =>
        {
            e.ToTable("PT_Skills");
            e.HasIndex(s => new { s.Name, s.IsDeleted }).IsUnique();
            e.HasIndex(s => new { s.Category, s.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateSkill>(e =>
        {
            e.ToTable("PT_CandidateSkills");
            e.HasIndex(cs => new { cs.PT_CandidateId, cs.PT_SkillId, cs.IsDeleted }).IsUnique();
            e.HasIndex(cs => new { cs.PT_SkillId, cs.IsDeleted });
        });

        modelBuilder.Entity<SYWizardStep>(e =>
        {
            e.ToTable("SY_WizardSteps");
            e.HasIndex(w => new { w.StepNumber, w.IsDeleted }).IsUnique();
            e.HasIndex(w => new { w.Order, w.Phase, w.IsDeleted });
        });

        modelBuilder.Entity<SYUserPreference>(e =>
        {
            e.ToTable("SY_UserPreferences");
            e.HasIndex(p => new { p.SCUserId, p.IsDeleted }).IsUnique();
            e.Property(p => p.Theme).HasDefaultValue("navy");
            e.Property(p => p.Language).HasDefaultValue("es");
        });

        modelBuilder.Entity<PTCandidate>(e =>
        {
            e.Property(c => c.IsProfilePublic).HasDefaultValue(true);
        });

        modelBuilder.Entity<PTCompany>(e =>
        {
            e.Property(c => c.IsVerified).HasDefaultValue(false);
        });

        modelBuilder.Entity<PTTempVacancy>(e =>
        {
            e.Property(v => v.WorkMode).HasDefaultValue(0);
        });

        modelBuilder.Entity<PTVacancy>(e =>
        {
            e.ToTable("PT_Vacancies");
            e.HasIndex(v => new { v.PT_CompanyId, v.IsDeleted });
            e.HasIndex(v => new { v.Status, v.IsDeleted });
            e.HasIndex(v => new { v.Location, v.Status, v.IsDeleted });
            e.HasIndex(v => new { v.Category, v.Status, v.IsDeleted });
            e.HasIndex(v => new { v.WorkMode, v.Status, v.IsDeleted });
            e.Property(v => v.Status).HasDefaultValue(0);
            e.Property(v => v.WorkMode).HasDefaultValue(0);
            e.Property(v => v.ViewsCount).HasDefaultValue(0);
            e.HasOne(v => v.JobType)
                .WithMany()
                .HasForeignKey(v => v.PT_JobTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PTApplication>(e =>
        {
            e.ToTable("PT_Applications");
            e.HasIndex(a => new { a.PT_CandidateId, a.PT_VacancyId, a.IsDeleted }).IsUnique();
            e.HasIndex(a => new { a.PT_VacancyId, a.Status, a.IsDeleted });
            e.HasIndex(a => new { a.PT_CandidateId, a.Status, a.IsDeleted });
            e.Property(a => a.Status).HasDefaultValue(0);
            e.Property(a => a.ApplicationSource).HasDefaultValue(0);
        });

        modelBuilder.Entity<PTVacancyContract>(e =>
        {
            e.ToTable("PT_VacancyContracts");
            e.HasIndex(c => new { c.PT_CompanyId, c.IsDeleted });
            e.Property(c => c.Status).HasDefaultValue(0);
            e.Property(c => c.Currency).HasDefaultValue("EUR");
            e.Property(c => c.FeeApplicationType).HasDefaultValue(0);
            e.Property(c => c.PaymentOpeningPct).HasDefaultValue(30m);
            e.Property(c => c.PaymentValidationPct).HasDefaultValue(50m);
            e.Property(c => c.PaymentConsolidationPct).HasDefaultValue(20m);
        });

        modelBuilder.Entity<PTContractVacancy>(e =>
        {
            e.ToTable("PT_ContractVacancies");
            e.HasIndex(cv => new { cv.PT_ContractId, cv.IsDeleted });
            e.HasIndex(cv => new { cv.PT_VacancyId, cv.IsDeleted }).IsUnique();
            e.HasOne(cv => cv.JobType)
                .WithMany()
                .HasForeignKey(cv => cv.PT_JobTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(cv => cv.PromoCode)
                .WithMany()
                .HasForeignKey(cv => cv.PT_PromoCodeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PTContractPayment>(e =>
        {
            e.ToTable("PT_ContractPayments");
            e.HasIndex(p => new { p.PT_VacancyContractId, p.TrancheType, p.IsDeleted });
            e.Property(p => p.Status).HasDefaultValue(0);
            e.HasOne(p => p.Contract)
                .WithMany()
                .HasForeignKey(p => p.PT_VacancyContractId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.PaidByUser)
                .WithMany()
                .HasForeignKey(p => p.PaidByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PTWarrantyReplacement>(e =>
        {
            e.ToTable("PT_WarrantyReplacements");
            e.HasIndex(w => new { w.PT_VacancyId, w.IsDeleted });
            e.Property(w => w.Status).HasDefaultValue(0);
            e.HasOne(w => w.Contract)
                .WithMany()
                .HasForeignKey(w => w.PT_VacancyContractId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.Vacancy)
                .WithMany()
                .HasForeignKey(w => w.PT_VacancyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.OriginalNegotiation)
                .WithMany()
                .HasForeignKey(w => w.OriginalNegotiationId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.OriginalDelivery)
                .WithMany()
                .HasForeignKey(w => w.OriginalDeliveryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.ReplacementNegotiation)
                .WithMany()
                .HasForeignKey(w => w.ReplacementNegotiationId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.ReplacementDelivery)
                .WithMany()
                .HasForeignKey(w => w.ReplacementDeliveryId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.ChargeTranche)
                .WithMany()
                .HasForeignKey(w => w.ChargeTrancheId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(w => w.RequestedByUser)
                .WithMany()
                .HasForeignKey(w => w.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SYSystemConfig>(e =>
        {
            e.ToTable("SY_SystemConfig");
            e.HasIndex(c => c.Key).IsUnique().HasFilter("IsDeleted = 0");
            e.Property(c => c.Category).HasDefaultValue("General");
            e.Property(c => c.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<PTJobLevel>(e =>
        {
            e.ToTable("PT_JobLevels");
            e.HasIndex(l => new { l.Name, l.IsDeleted });
            e.Property(l => l.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<PTJobType>(e =>
        {
            e.ToTable("PT_JobTypes");
            e.HasIndex(t => new { t.PT_JobLevelId, t.IsDeleted });
            e.HasIndex(t => new { t.Name, t.IsDeleted });
            e.Property(t => t.IsActive).HasDefaultValue(true);
            e.HasOne(t => t.JobLevel)
                .WithMany(l => l.JobTypes)
                .HasForeignKey(t => t.PT_JobLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PTJobTypePrice>(e =>
        {
            e.ToTable("PT_JobTypePrices");
            e.HasIndex(p => new { p.PT_JobTypeId, p.EffectiveTo, p.IsDeleted });
            e.Property(p => p.Currency).HasDefaultValue("EUR");
            e.HasOne(p => p.JobType)
                .WithMany(t => t.Prices)
                .HasForeignKey(p => p.PT_JobTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PTPromoCode>(e =>
        {
            e.ToTable("PT_PromoCodes");
            e.HasIndex(p => new { p.Code, p.IsDeleted }).IsUnique();
            e.Property(p => p.IsActive).HasDefaultValue(true);
            e.Property(p => p.UsesCount).HasDefaultValue(0);
            e.HasOne(p => p.JobLevel)
                .WithMany()
                .HasForeignKey(p => p.PT_JobLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.JobType)
                .WithMany()
                .HasForeignKey(p => p.PT_JobTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PTPromoCodeRedemption>(e =>
        {
            e.ToTable("PT_PromoCodeRedemptions");
            e.HasIndex(r => new { r.PT_PromoCodeId, r.IsDeleted });
            e.HasOne(r => r.PromoCode)
                .WithMany(p => p.Redemptions)
                .HasForeignKey(r => r.PT_PromoCodeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.ContractVacancy)
                .WithMany()
                .HasForeignKey(r => r.PT_ContractVacancyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PTCandidateExperience>(e =>
        {
            e.ToTable("PT_CandidateExperiences");
            e.HasIndex(exp => new { exp.PT_CandidateId, exp.IsDeleted });
            e.HasIndex(exp => new { exp.CompanyName, exp.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateEducation>(e =>
        {
            e.ToTable("PT_CandidateEducations");
            e.HasIndex(edu => new { edu.PT_CandidateId, edu.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateCertification>(e =>
        {
            e.ToTable("PT_CandidateCertifications");
            e.HasIndex(cert => new { cert.PT_CandidateId, cert.IsDeleted });
        });

        modelBuilder.Entity<PTVacancySkill>(e =>
        {
            e.ToTable("PT_VacancySkills");
            e.HasIndex(vs => new { vs.PT_VacancyId, vs.PT_SkillId, vs.IsDeleted }).IsUnique();
            e.HasIndex(vs => new { vs.PT_SkillId, vs.IsDeleted });
            e.Property(vs => vs.IsRequired).HasDefaultValue(true);
        });

        modelBuilder.Entity<PTJobTypeSkill>(e =>
        {
            e.ToTable("PT_JobTypeSkills");
            e.HasIndex(s => new { s.PT_JobTypeId, s.PT_SkillId, s.IsDeleted }).IsUnique();
            e.Property(s => s.IsRequired).HasDefaultValue(true);
            e.HasOne(s => s.JobType)
                .WithMany(t => t.DefaultSkills)
                .HasForeignKey(s => s.PT_JobTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Skill)
                .WithMany()
                .HasForeignKey(s => s.PT_SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ADAuditLog>(e =>
        {
            e.ToTable("AD_AuditLogs");
            e.HasIndex(a => new { a.SCUserId, a.IsDeleted });
            e.HasIndex(a => new { a.EntityType, a.EntityId, a.IsDeleted });
            e.HasIndex(a => new { a.CreatedAt, a.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateRecruitment>(e =>
        {
            e.ToTable("PT_CandidateRecruitments");
            e.HasIndex(r => new { r.SCUserId, r.IsDeleted });
            e.HasIndex(r => new { r.CurrentStage, r.IsDeleted });
            e.HasIndex(r => new { r.AssignedToUserId, r.IsDeleted });
            e.HasIndex(r => new { r.SCUserId, r.PT_VacancyId, r.IsDeleted });
            e.Property(r => r.CurrentStage).HasDefaultValue(0);
        });

        modelBuilder.Entity<PTRecruitmentStageLog>(e =>
        {
            e.ToTable("PT_RecruitmentStageLogs");
            e.HasIndex(l => new { l.PT_CandidateRecruitmentId, l.IsDeleted });
            e.HasIndex(l => new { l.CreatedAt, l.IsDeleted });
        });

        modelBuilder.Entity<PTInvestigationChecklist>(e =>
        {
            e.ToTable("PT_InvestigationChecklists");
            e.HasIndex(c => new { c.PT_CandidateRecruitmentId, c.Step, c.IsDeleted }).IsUnique();
            e.HasIndex(c => new { c.IsCompleted, c.IsDeleted });
            e.Property(c => c.IsCompleted).HasDefaultValue(false);
            e.HasMany(c => c.ReferenceChecks)
                .WithOne(r => r.Checklist)
                .HasForeignKey(r => r.PT_InvestigationChecklistId);
        });

        modelBuilder.Entity<PTReferenceCheck>(e =>
        {
            e.ToTable("PT_ReferenceChecks");
            e.HasIndex(r => new { r.PT_InvestigationChecklistId, r.IsDeleted });
        });

        modelBuilder.Entity<PTTechnicalEvaluation>(e =>
        {
            e.ToTable("PT_TechnicalEvaluations");
            e.HasIndex(t => new { t.PT_CandidateRecruitmentId, t.IsDeleted });
            e.HasIndex(t => new { t.EvaluatedByUserId, t.IsDeleted });
        });

        modelBuilder.Entity<PTRecruitmentDismissal>(e =>
        {
            e.ToTable("PT_RecruitmentDismissals");
            e.HasIndex(d => new { d.PT_CandidateRecruitmentId, d.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateRecruitmentPreferences>(e =>
        {
            e.ToTable("PT_CandidateRecruitmentPreferences");
            e.HasIndex(p => new { p.PT_CandidateRecruitmentId, p.IsDeleted }).IsUnique();
            e.HasIndex(p => new { p.IsCompleted, p.IsDeleted });
            e.Property(p => p.IsCompleted).HasDefaultValue(false);
            e.HasOne(p => p.Recruitment)
                .WithOne(r => r.Preferences)
                .HasForeignKey<PTCandidateRecruitmentPreferences>(p => p.PT_CandidateRecruitmentId);
        });

        modelBuilder.Entity<SYDocumentType>(e =>
        {
            e.ToTable("SY_DocumentTypes");
            e.HasIndex(d => new { d.Name, d.IsDeleted }).IsUnique();
            e.HasIndex(d => new { d.Category, d.IsDeleted });
            e.Property(d => d.IsRequired).HasDefaultValue(false);
            e.HasMany(d => d.RecruitmentDocuments)
                .WithOne(r => r.DocumentType)
                .HasForeignKey(r => r.SY_DocumentTypeId);
        });

        modelBuilder.Entity<PTRecruitmentDocument>(e =>
        {
            e.ToTable("PT_RecruitmentDocuments");
            e.HasIndex(r => new { r.PT_CandidateRecruitmentId, r.SY_DocumentTypeId, r.IsDeleted }).IsUnique();
            e.HasIndex(r => new { r.Status, r.IsDeleted });
            e.HasIndex(r => new { r.PT_CandidateRecruitmentId, r.IsDeleted });
            e.Property(r => r.Status).HasDefaultValue(0);
            e.HasOne(r => r.Recruitment)
                .WithMany(rec => rec.RecruitmentDocuments)
                .HasForeignKey(r => r.PT_CandidateRecruitmentId);
        });

        SeedDocumentTypes(modelBuilder);

        modelBuilder.Entity<PTCandidateScore>(e =>
        {
            e.ToTable("PT_CandidateScores");
            e.HasIndex(s => new { s.PT_CandidateId, s.IsDeleted }).IsUnique();
        });

        modelBuilder.Entity<PTJobMatchScore>(e =>
        {
            e.ToTable("PT_JobMatchScores");
            e.HasIndex(m => new { m.PT_CandidateId, m.PT_VacancyId, m.IsDeleted }).IsUnique();
            e.HasIndex(m => new { m.PT_VacancyId, m.MatchPercentage, m.IsDeleted });
        });

        modelBuilder.Entity<PTVerification>(e =>
        {
            e.ToTable("PT_Verifications");
            e.HasIndex(v => new { v.PT_CandidateId, v.Type, v.IsDeleted }).IsUnique();
            e.HasIndex(v => new { v.Status, v.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateReference>(e =>
        {
            e.ToTable("PT_CandidateReferences");
            e.HasIndex(r => new { r.PT_CandidateId, r.IsDeleted });
            e.HasIndex(r => r.TokenHash); // lookup del endpoint publico de feedback (3.5).
        });

        modelBuilder.Entity<PTSkillTest>(e =>
        {
            e.ToTable("PT_SkillTests");
            e.HasIndex(t => new { t.Category, t.IsActive, t.IsDeleted });
        });

        modelBuilder.Entity<PTCandidateTestResult>(e =>
        {
            e.ToTable("PT_CandidateTestResults");
            e.HasIndex(r => new { r.PT_CandidateId, r.IsDeleted });
            e.HasIndex(r => new { r.PT_SkillTestId, r.IsDeleted });
        });

        modelBuilder.Entity<PTNegotiation>(e =>
        {
            e.ToTable("PT_Negotiations");
            e.HasIndex(n => new { n.PT_VacancyId, n.IsDeleted });
            e.HasIndex(n => new { n.Status, n.IsDeleted });
            e.HasOne(n => n.AssignedStaff)
                .WithMany()
                .HasForeignKey(n => n.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(n => n.WinningApplication)
                .WithMany()
                .HasForeignKey(n => n.WinningApplicationId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(n => n.ProcessClosedByUser)
                .WithMany()
                .HasForeignKey(n => n.ProcessClosedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(n => n.FeedbackRecordedByUser)
                .WithMany()
                .HasForeignKey(n => n.FeedbackRecordedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PTNegotiationCandidate>(e =>
        {
            e.ToTable("PT_NegotiationCandidates");
            e.HasIndex(nc => new { nc.PT_NegotiationId, nc.PT_ApplicationId, nc.IsDeleted }).IsUnique();
            e.HasOne(nc => nc.Negotiation)
                .WithMany(n => n.Candidates)
                .HasForeignKey(nc => nc.PT_NegotiationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(nc => nc.Application)
                .WithMany()
                .HasForeignKey(nc => nc.PT_ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PTCompany>(e =>
        {
            e.ToTable("PT_Companies");
            e.HasIndex(c => new { c.Name, c.IsDeleted });
            e.HasIndex(c => new { c.Status, c.IsDeleted });
            // Unico por usuario, pero SCUserId es nullable: MySQL permite multiples
            // filas con NULL en un indice unico, asi que los prospectos del CRM
            // (SCUserId = null) no colisionan entre si.
            e.HasIndex(c => c.SCUserId).IsUnique();
            e.HasOne(c => c.User)
                .WithOne(u => u.Company)
                .HasForeignKey<PTCompany>(c => c.SCUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PTCompanyPipeline>(e =>
        {
            e.ToTable("PT_CompanyPipelines");
            e.HasIndex(p => new { p.PT_CompanyId, p.IsDeleted });
            e.HasIndex(p => new { p.CurrentStage, p.IsDeleted });
            e.HasIndex(p => new { p.AssignedToUserId, p.IsDeleted });
            e.HasIndex(p => new { p.IsDismissed, p.IsDeleted });
            e.HasOne(p => p.Company)
                .WithMany(c => c.Pipelines)
                .HasForeignKey(p => p.PT_CompanyId);
        });

        modelBuilder.Entity<PTPlan>(e =>
        {
            e.ToTable("PT_Plans");
            e.HasIndex(p => new { p.IsActive, p.IsDeleted });
        });

        modelBuilder.Entity<PTCompanyStageLog>(e =>
        {
            e.ToTable("PT_CompanyStageLogs");
            e.HasIndex(s => new { s.PT_CompanyPipelineId, s.IsDeleted });
            e.HasIndex(s => s.ChangedByUserId);
            e.HasOne(s => s.Pipeline)
                .WithMany(p => p.StageLogs)
                .HasForeignKey(s => s.PT_CompanyPipelineId);
        });

        modelBuilder.Entity<PTCandidateDelivery>(e =>
        {
            e.ToTable("PT_CandidateDeliveries");
            e.HasIndex(d => new { d.PT_CompanyId, d.IsDeleted });
            e.HasIndex(d => new { d.PT_VacancyId, d.IsDeleted });
            e.HasIndex(d => new { d.PT_CandidateRecruitmentId, d.IsDeleted });
            e.HasIndex(d => new { d.Status, d.IsDeleted });
            e.HasOne(d => d.Recruitment)
                .WithMany()
                .HasForeignKey(d => d.PT_CandidateRecruitmentId);
            e.HasOne(d => d.Candidate)
                .WithMany()
                .HasForeignKey(d => d.PT_CandidateId);
            e.HasOne(d => d.Vacancy)
                .WithMany()
                .HasForeignKey(d => d.PT_VacancyId);
            e.HasOne(d => d.Company)
                .WithMany()
                .HasForeignKey(d => d.PT_CompanyId);
            e.HasOne(d => d.DeliveredByUser)
                .WithMany()
                .HasForeignKey(d => d.DeliveredByUserId);
            e.HasOne(d => d.ProcessClosedByUser)
                .WithMany()
                .HasForeignKey(d => d.ProcessClosedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(d => d.FeedbackRecordedByUser)
                .WithMany()
                .HasForeignKey(d => d.FeedbackRecordedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        SeedWizardSteps(modelBuilder);
        SeedPlans(modelBuilder);
    }

    private static void SeedPlans(ModelBuilder modelBuilder)
    {
        var plans = new[]
        {
            new PTPlan { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), Name = "Basic", Description = "Plan básico con funcionalidades esenciales para empezar.", Price = 49.00m, Currency = "EUR", SortOrder = 1, IsActive = true },
            new PTPlan { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), Name = "Premium", Description = "Plan premium con herramientas avanzadas de gestión y soporte prioritario.", Price = 99.00m, Currency = "EUR", SortOrder = 2, IsActive = true },
            new PTPlan { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), Name = "Platinum", Description = "Plan platinum con todas las funcionalidades, soporte dedicado y personalización total.", Price = 199.00m, Currency = "EUR", SortOrder = 3, IsActive = true }
        };

        modelBuilder.Entity<PTPlan>().HasData(plans);
    }

    private static void SeedDocumentTypes(ModelBuilder modelBuilder)
    {
        var docs = new[]
        {
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Pasaporte", Description = "Pasaporte válido y en vigor", Category = "Identidad", IsRequired = false, SortOrder = 1 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Documento de identidad", Description = "DNI / NIE / Cédula de identidad", Category = "Identidad", IsRequired = true, SortOrder = 2 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Permiso de trabajo", Description = "Autorización de trabajo en el país de destino", Category = "Migratorio", IsRequired = false, SortOrder = 3 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Licencia de conducir", Description = "Permiso de conducir válido", Category = "Habilitación", IsRequired = false, SortOrder = 4 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Visado de trabajo", Description = "Visado que habilita a trabajar legalmente", Category = "Migratorio", IsRequired = false, SortOrder = 5 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Tarjeta sanitaria", Description = "Tarjeta sanitaria europea (TSE) o seguro médico privado", Category = "Salud", IsRequired = false, SortOrder = 6 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Certificado de antecedentes penales", Description = "Certificado de antecedentes penales apostillado", Category = "Legal", IsRequired = false, SortOrder = 7 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Titulo / Certificación profesional", Description = "Título habilitante o certificación profesional", Category = "Formación", IsRequired = false, SortOrder = 8 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Nº Seguridad Social", Description = "Documento con número de afiliación a la seguridad social", Category = "Fiscal", IsRequired = false, SortOrder = 9 },
            new SYDocumentType { Id = Guid.NewGuid(), Name = "Cuenta bancaria (IBAN)", Description = "Justificante de cuenta bancaria a nombre del candidato", Category = "Fiscal", IsRequired = false, SortOrder = 10 }
        };

        modelBuilder.Entity<SYDocumentType>().HasData(docs);
    }

    private static void SeedWizardSteps(ModelBuilder modelBuilder)
    {
        var steps = new[]
        {
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 1, StepName = "PersonalData", StepTitle = "Personal Data", Description = "Tell us about yourself", IsRequired = true, Order = 1, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 2, StepName = "Location", StepTitle = "Location", Description = "Where are you located?", IsRequired = true, Order = 2, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 3, StepName = "ProfessionalProfile", StepTitle = "Professional Profile", Description = "Your professional information", IsRequired = true, Order = 3, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 4, StepName = "Skills", StepTitle = "Skills", Description = "Select your skills", IsRequired = false, Order = 4, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 5, StepName = "Preferences", StepTitle = "What do you want to do?", Description = "Choose your preference", IsRequired = true, Order = 5, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 6, StepName = "Confirmation", StepTitle = "Review and Confirm", Description = "Verify your data is correct", IsRequired = true, Order = 6, Phase = 1 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 7, StepName = "WorkExperience", StepTitle = "Work Experience", Description = "Add your work experience", IsRequired = false, Order = 7, Phase = 2 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 8, StepName = "Education", StepTitle = "Education", Description = "Add your education", IsRequired = false, Order = 8, Phase = 2 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 9, StepName = "Certifications", StepTitle = "Certifications", Description = "Add your certifications", IsRequired = false, Order = 9, Phase = 2 },
            new SYWizardStep { Id = Guid.NewGuid(), StepNumber = 10, StepName = "UploadCV", StepTitle = "Upload CV", Description = "Upload your CV/resume", IsRequired = false, Order = 10, Phase = 2 }
        };

        modelBuilder.Entity<SYWizardStep>().HasData(steps);
    }
}
