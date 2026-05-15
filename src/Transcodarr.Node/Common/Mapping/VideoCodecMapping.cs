using FFMpegCore.Enums;

namespace Transcodarr.Node.Common.Mapping;

public static class VideoCodecMapping
{
    public static Codec Map(this Transcodearr.Shared.DTOs.VideoCodec codec)
    {
        return codec switch
        {
            Transcodearr.Shared.DTOs.VideoCodec.H264 => VideoCodec.LibX264,
            Transcodearr.Shared.DTOs.VideoCodec.H265 => VideoCodec.LibX265,
            Transcodearr.Shared.DTOs.VideoCodec.Av1 => VideoCodec.LibaomAv1,
            _ => throw new ArgumentOutOfRangeException(nameof(codec), codec, null)
        };
    }
}