using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Database.Entities;

public class TranscodeJobEntity
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }
    public MediaFileEntity MediaFile { get; set; } = null!;

    public required string NodeId { get; set; }

    public required string OutputPath { get; set; }
    public int ConstantRateFactor { get; set; }
    public AudioCodec AudioCodec { get; set; }
    public VideoCodec VideoCodec { get; set; }
    public EncoderPreset EncoderPreset { get; set; }
    public TranscodeJobStatus Status { get; set; }

    public double Progress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LeaseExpiresAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public TranscodeResultEntity? TranscodeResult { get; set; }
}
