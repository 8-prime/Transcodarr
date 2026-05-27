using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Database.Entities;

public class TranscodeJobEntity
{
    public Guid Id { get; init; }
    public Guid MediaFileId { get; init; }
    public MediaFileEntity MediaFile { get; init; } = null!;

    public required string NodeId { get; init; }

    public required string OutputPath { get; init; }
    public int ConstantRateFactor { get; init; }
    public AudioCodec AudioCodec { get; init; }
    public VideoCodec VideoCodec { get; init; }
    public EncoderPreset EncoderPreset { get; init; }
    public TranscodeJobStatus Status { get; set; }

    public double Progress { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LeaseExpiresAt { get; set; }
    public DateTimeOffset? CompletedAt { get; init; }
    public TranscodeResultEntity? TranscodeResult { get; init; }
}
