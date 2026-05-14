using System.ComponentModel.DataAnnotations;

namespace Transcodarr.Core.Database.Entities;

public class AppConfigurationEntity
{
    public bool AutoApplyTranscode { get; set; }
    [MaxLength(4096)] public required string TranscodeTempDirectory { get; set; }
}