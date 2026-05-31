using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.MediaFiles;

public class TranscodeEligibilityService
{
    public bool IsEligibleForTranscode(FileProbeResult fileInfoEntity)
    {
        return true;
    }
}
