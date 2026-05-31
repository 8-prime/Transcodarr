using Transcodarr.Shared;
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

    public static readonly Dictionary<string, EncoderGroup> EncoderSlotGroup = new()
    {
        { "libx264", EncoderGroup.Software },
        { "libx265", EncoderGroup.Software },
        { "libaom-av1", EncoderGroup.Software },
        { "h264_nvenc", EncoderGroup.Nvenc },
        { "hevc_nvenc", EncoderGroup.Nvenc },
        { "av1_nvenc", EncoderGroup.Nvenc },
        { "h264_amf", EncoderGroup.Amf },
        { "hevc_amf", EncoderGroup.Amf },
        { "av1_amf", EncoderGroup.Amf },
        { "h264_sqv", EncoderGroup.Qsv },
        { "hevc_sqv", EncoderGroup.Qsv },
        { "av1_sqv", EncoderGroup.Qsv },
    };

    public static EncoderGroup GetSlotGroup(string encoder) =>
        EncoderSlotGroup.GetValueOrDefault(encoder, EncoderGroup.Software);

    public static bool EncoderMatchesCodec(string encoder, VideoCodec codec)
    {
        return EncodersByCodec.TryGetValue(codec, out var encoders) && encoders.Contains(encoder);
    }
}
