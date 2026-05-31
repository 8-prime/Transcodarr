using Transcodarr.Core.Common.DTOs.Enums;

namespace Transcodarr.Core.Common.DTOs;

public class QueueItemResponse
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string LibraryName { get; init; }
    public required string TargetCodec { get; init; }
    public QueueItemStatus State { get; init; }
    public required int AttemptNumber { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? NodeId { get; init; }
    public double? ProgressPct { get; init; }
}
