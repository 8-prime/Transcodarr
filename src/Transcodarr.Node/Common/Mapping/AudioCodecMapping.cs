using FFMpegCore.Enums;

namespace Transcodarr.Node.Common.Mapping;

public static class AudioCodecMapping
{
    public static Codec Map(this Transcodarr.Shared.DTOs.AudioCodec audioCodec)
    {
        return audioCodec switch
        {
            Transcodarr.Shared.DTOs.AudioCodec.Aac => AudioCodec.Aac,
            Transcodarr.Shared.DTOs.AudioCodec.Ac3 => AudioCodec.Ac3,
            Transcodarr.Shared.DTOs.AudioCodec.Copy => AudioCodec.Copy,
            _ => throw new ArgumentOutOfRangeException(nameof(audioCodec), audioCodec, null),
        };
    }
}
