namespace Transcodarr.Core.Database.Enums;

public enum ProcessingState
{
    Discovered,
    Probing,
    Queued,
    Processing,
    Validating,
    Ignored,
    Failed,
    Succeeded
}