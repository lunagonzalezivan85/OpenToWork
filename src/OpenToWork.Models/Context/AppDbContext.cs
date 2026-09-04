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
        SeedWizardSteps(modelBuilder);
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
