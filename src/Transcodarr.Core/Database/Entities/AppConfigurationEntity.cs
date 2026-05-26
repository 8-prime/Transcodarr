using System.ComponentModel.DataAnnotations;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Core.Database.Entities;

public class AppConfigurationEntity
{
    public Guid Id { get; set; }
    public bool AutoApplyTranscode { get; set; }

    [MaxLength(4096)]
    public required string TranscodeTempDirectory { get; set; }
    public AudioCodec TranscodeAudioCodec { get; set; }
    public EncoderPreset TranscodeEncoderPreset { get; set; }
    public VideoCodec TranscodeVideoCodec { get; set; }
    public int ConstantRateFactor { get; set; }
    public int JobExpirationInMinutes { get; set; }
}
