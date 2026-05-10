using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class TranscodeJob
{
    public Guid Id { get; set; }
    public Guid FileInfoId { get; set; }
    public Guid LeaseToken { get; set; }
    public required string NodeId { get; set; }
    public JobState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LeaseExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptNumber { get; set; }
    
    // Encode results
    public long? OutputSizeBytes { get; set; }
    public double? VmafScore { get; set; }
    public string? EncoderSettingsSnapshot { get; set; }
}