using System.ComponentModel.DataAnnotations;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class TranscodeJobEntity
{
    public Guid Id { get; set; }
    public Guid FileInfoId { get; set; }
    public JobState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? LeaseToken { get; set; }
    public DateTime? LeaseExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    [MaxLength(4096)] public required string OutputPath { get; set; }
    [MaxLength(100)] public string? NodeId { get; set; }
    [MaxLength(256)] public string? ErrorMessage { get; set; }
    public int AttemptNumber { get; set; }

    // Encode results
    public long? OutputSizeBytes { get; set; }
    public double? VmafScore { get; set; }
    [MaxLength(256)] public string? EncoderSettingsSnapshot { get; set; }
}