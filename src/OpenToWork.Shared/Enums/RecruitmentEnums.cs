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
    CallCandidate = 0,
    CallReferences = 1,
    ValidateLinkedIn = 2,
    ValidatePortfolio = 3,
    ValidateCertifications = 4
}

public enum DismissalReason
{
    Technical = 0,
    References = 1,
    Salary = 2,
    NoShow = 3,
    Other = 4
}
