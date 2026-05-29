namespace Transcodarr.Core.Database.Enums;

public enum TranscodeJobStatus
{
    Processing,
    Completed,
    LeaseExpired,
    Failed,
}
