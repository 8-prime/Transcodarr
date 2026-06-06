using FFMpegCore.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Common.Mapping;

public static class EncoderPresetMapping
{
    public static Speed Map(this EncoderPreset encoderPreset) =>
        encoderPreset switch
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

    public static string MapToNvenc(this EncoderPreset encoderPreset) =>
        encoderPreset switch
        {
            EncoderPreset.Ultrafast => "p1",
            EncoderPreset.Superfast => "p1",
            EncoderPreset.Veryfast => "p2",
            EncoderPreset.Faster => "p3",
            EncoderPreset.Fast => "p4",
            EncoderPreset.Medium => "p5",
            EncoderPreset.Slow => "p6",
            EncoderPreset.Slower => "p6",
            EncoderPreset.Veryslow => "p7",
            _ => throw new ArgumentOutOfRangeException(nameof(encoderPreset), encoderPreset, null),
        };

    public static string MapToQsv(this EncoderPreset encoderPreset) =>
        encoderPreset switch
        {
            EncoderPreset.Ultrafast => "veryfast",
            EncoderPreset.Superfast => "veryfast",
            EncoderPreset.Veryfast => "veryfast",
            EncoderPreset.Faster => "faster",
            EncoderPreset.Fast => "fast",
            EncoderPreset.Medium => "medium",
            EncoderPreset.Slow => "slow",
            EncoderPreset.Slower => "slower",
            EncoderPreset.Veryslow => "veryslow",
            _ => throw new ArgumentOutOfRangeException(nameof(encoderPreset), encoderPreset, null),
        };
}
