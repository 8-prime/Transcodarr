using System.ComponentModel.DataAnnotations;

namespace Transcodarr.Core.Database.Entities;

public class ProcessingResultEntity
{
    public long OutputSizeBytes { get; set; }
    public double VmafScore { get; set; }
    [MaxLength(256)] public string? EncoderSettingsSnapshot { get; set; }
}