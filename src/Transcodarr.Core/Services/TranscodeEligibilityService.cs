using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database.Entities;

namespace Transcodarr.Core.Services;

public class TranscodeEligibilityService
{
    public bool IsEligibleForTranscode(FileProbeResult fileInfoEntity)
    {
        return true;
    }
}