using Transcodarr.Core.Common.DTOs.Enums;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Common.Extensions;

public static class QueueItemMapping
{
    extension(QueueItemStatus)
    {
        public static QueueItemStatus FromMediaFileStatus(TranscodeStatus transcodeStatus)
        {
            return transcodeStatus switch
            {
                TranscodeStatus.Discovered => QueueItemStatus.Discovered,
                TranscodeStatus.Pending => QueueItemStatus.Pending,
                TranscodeStatus.InProgress => QueueItemStatus.Processing,
                TranscodeStatus.NotRequired => QueueItemStatus.Completed,
                TranscodeStatus.Completed => QueueItemStatus.Completed,
                TranscodeStatus.Failed => QueueItemStatus.Failed,
                TranscodeStatus.Completing => QueueItemStatus.Processing,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(transcodeStatus),
                    transcodeStatus,
                    null
                ),
            };
        }
    }
}
