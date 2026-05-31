namespace Transcodarr.Core.Common.DTOs.Enums;

public enum QueueItemStatus
{
    Discovered,
    Pending,
    Processing,
    Completed,
    Failed,
    LeaseExpired,
}
