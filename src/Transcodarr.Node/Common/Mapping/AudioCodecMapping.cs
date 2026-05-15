using FFMpegCore.Enums;

namespace Transcodarr.Node.Common.Mapping;

public static class AudioCodecMapping
{
    public static Codec Map(this Transcodearr.Shared.DTOs.AudioCodec audioCodec)
    {
        return audioCodec switch
        {
            Transcodearr.Shared.DTOs.AudioCodec.Aac => AudioCodec.Aac,
            Transcodearr.Shared.DTOs.AudioCodec.Ac3 => AudioCodec.Ac3,
            Transcodearr.Shared.DTOs.AudioCodec.Copy => AudioCodec.Copy,
            _ => throw new ArgumentOutOfRangeException(nameof(audioCodec), audioCodec, null)
        };
    }
}