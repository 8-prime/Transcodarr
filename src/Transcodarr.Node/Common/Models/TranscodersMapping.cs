using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Common.Models;

public static class TranscodersMapping
{
    public static readonly Dictionary<VideoCodec, HashSet<string>> EncodersByCodec = new()
    {
        { VideoCodec.H264, ["libx264", "h264_nvenc", "h264_sqv", "h264_amf"] },
        { VideoCodec.H265, ["libx265", "hevc_nvenc", "hevc_sqv", "hevc_amf"] },
        { VideoCodec.H265, ["libaom-av1", "av1_nvenc", "av1_sqv", "av1_amf"] },
    };

    public static bool EncoderMatchesCodec(string encoder, VideoCodec codec)
    {
        return EncodersByCodec.TryGetValue(codec, out var encoders) && encoders.Contains(encoder);
    }
}
