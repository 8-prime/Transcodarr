using FFMpegCore.Enums;

namespace Transcodarr.Node.Common.Mapping;

public static class VideoCodecMapping
{
    public static Codec Map(this Transcodarr.Shared.DTOs.VideoCodec codec)
    {
        return codec switch
        {
            Transcodarr.Shared.DTOs.VideoCodec.H264 => VideoCodec.LibX264,
            Transcodarr.Shared.DTOs.VideoCodec.H265 => VideoCodec.LibX265,
            Transcodarr.Shared.DTOs.VideoCodec.Av1 => VideoCodec.LibaomAv1,
            _ => throw new ArgumentOutOfRangeException(nameof(codec), codec, null),
        };
    }
}
