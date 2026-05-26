namespace Transcodarr.Shared.DTOs;

public record TranscoderSnapshot(
    string EncoderName,
    AudioCodec AudioCodec,
    VideoCodec VideoCodec,
    EncoderPreset EncoderPreset,
    int ConstantRateFactor
);
