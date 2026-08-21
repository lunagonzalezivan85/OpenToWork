namespace OpenToWork.Shared.Enums;

public enum RecruitmentStage
{
    Postulation = 0,
    Investigation = 1,
    TechnicalEvaluation = 2,
    CulturalInterview = 3,
    ReadyToDeliver = 4,
    Dismissed = 5
}

public enum InvestigationStep
{
    IdentityValidation = 0,
    WorkHistoryAudit = 1,
    CredentialsVerification = 2,
    LegalBackground = 3,
    SalaryAgreement = 4
}

public enum DismissalReason
{
    Technical = 0,
    References = 1,
    Salary = 2,
    NoShow = 3,
    Other = 4
}
