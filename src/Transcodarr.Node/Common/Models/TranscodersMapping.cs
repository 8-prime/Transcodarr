using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Common.Models;

public static class TranscodersMapping
{
    public static readonly Dictionary<VideoCodec, HashSet<string>> EncodersByCodec = new()
    {
        { VideoCodec.H264, ["libx264", "h264_nvenc", "h264_sqv", "h264_amf"] },
        { VideoCodec.H265, ["libx265", "hevc_nvenc", "hevc_sqv", "hevc_amf"] },
        { VideoCodec.Av1, ["libaom-av1", "av1_nvenc", "av1_sqv", "av1_amf"] },
    };

    public static readonly Dictionary<string, string> EncoderSlotGroup = new()
    {
        { "libx264", "software" },
        { "libx265", "software" },
        { "libaom-av1", "software" },
        { "h264_nvenc", "nvenc" },
        { "hevc_nvenc", "nvenc" },
        { "av1_nvenc", "nvenc" },
        { "h264_amf", "amf" },
        { "hevc_amf", "amf" },
        { "av1_amf", "amf" },
        { "h264_sqv", "qsv" },
        { "hevc_sqv", "qsv" },
        { "av1_sqv", "qsv" },
    };

    public static string GetSlotGroup(string encoder) =>
        EncoderSlotGroup.GetValueOrDefault(encoder, "software");

    public static bool EncoderMatchesCodec(string encoder, VideoCodec codec)
    {
        return EncodersByCodec.TryGetValue(codec, out var encoders) && encoders.Contains(encoder);
    }
}
