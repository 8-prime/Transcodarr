using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class TranscodeResultEntity
{
    public Guid Id { get; set; }
    public Guid TranscodeJobId { get; set; }
    public TranscodeJobEntity TranscodeJob { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public double? VmafScore { get; set; }
    public ApprovalState ApprovalState { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}