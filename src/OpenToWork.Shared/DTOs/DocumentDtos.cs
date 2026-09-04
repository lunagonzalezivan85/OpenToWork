namespace OpenToWork.Shared.DTOs;

public class DocumentTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

public class RecruitmentDocumentDto
{
    public Guid Id { get; set; }
    public Guid RecruitmentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? DocumentTypeCategory { get; set; }
    public int Status { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class RequestDocumentDto
{
    public Guid DocumentTypeId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDocumentStatusDto
{
    public int Status { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? Notes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateMigrationInfoDto
{
    public string? Nationality { get; set; }
    public bool? HasPassport { get; set; }
    public string? PassportNumber { get; set; }
    public int? WorkAuthorization { get; set; }
    public string? WorkAuthorizations { get; set; }
    public bool? HasTransport { get; set; }
}
