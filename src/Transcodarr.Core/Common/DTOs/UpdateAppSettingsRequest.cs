using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Common.DTOs;

public class UpdateAppSettingsRequest
{
    public required VideoCodec VideoCodec { get; init; }
    public required AudioCodec AudioCodec { get; init; }
    public required EncoderPreset EncoderPreset { get; init; }
    public required int ConstantRateFactor { get; init; }
    public required bool AutoApplyTranscode { get; init; }
    public required int JobExpirationInMinutes { get; init; }
    public required string TranscodeTempDirectory { get; init; }
}
