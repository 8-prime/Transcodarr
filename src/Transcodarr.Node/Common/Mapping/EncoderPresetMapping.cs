using FFMpegCore.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Common.Mapping;

public static class EncoderPresetMapping
{
    public static Speed Map(this EncoderPreset encoderPreset)
    {
        return encoderPreset switch
        {
            EncoderPreset.Ultrafast => Speed.UltraFast,
            EncoderPreset.Superfast => Speed.SuperFast,
            EncoderPreset.Veryfast => Speed.VeryFast,
            EncoderPreset.Faster => Speed.Faster,
            EncoderPreset.Fast => Speed.Fast,
            EncoderPreset.Medium => Speed.Medium,
            EncoderPreset.Slow => Speed.Slow,
            EncoderPreset.Slower => Speed.Slower,
            EncoderPreset.Veryslow => Speed.VerySlow,
            _ => throw new ArgumentOutOfRangeException(nameof(encoderPreset), encoderPreset, null),
        };
    }
}
