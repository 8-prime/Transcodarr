using System.ComponentModel.DataAnnotations;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Core.Database.Entities;

public class AppConfigurationEntity
{
    public bool AutoApplyTranscode { get; set; }
    [MaxLength(4096)] public required string TranscodeTempDirectory { get; set; }
    public AudioCodec TranscodeTempAudioCodec { get; set; }
    public EncoderPreset TranscodeEncoderPreset { get; set; }
    public VideoCodec TranscodeTempVideoCodec { get; set; }
    public int ConstantRateFactor  { get; set; }
}