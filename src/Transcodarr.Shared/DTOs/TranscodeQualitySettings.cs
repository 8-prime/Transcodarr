using System.Text.Encodings.Web;

namespace Transcodearr.Shared.DTOs;

public class TranscodeQualitySettings
{
    public AudioCodec DesiredAudioCodec { get; set; }
    public VideoCodec DesiredVideoCodec { get; set; }
    public EncoderPreset DesiredEncoderPreset { get; set; }
    public int ConstantRateFactor { get; set; }
}