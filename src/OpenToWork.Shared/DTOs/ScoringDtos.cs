namespace OpenToWork.Shared.DTOs;

public class CandidateScoreDto
{
    public Guid CandidateId { get; set; }
    public int StabilityIndex { get; set; }
    public int ReliabilityIndex { get; set; }
    public int EvidenceIndex { get; set; }
    public int CompatibilityIndex { get; set; }
    public int OverallScore { get; set; }
    public DateTime CalculatedAt { get; set; }
    public int Version { get; set; }
}
