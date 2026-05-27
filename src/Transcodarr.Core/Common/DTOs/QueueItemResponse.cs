using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Common.DTOs;

public class QueueItemResponse
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string LibraryName { get; init; }
    public required string NodeId { get; init; }
    public required TranscodeJobStatus Status { get; init; }
    public required double Progress { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset LeaseExpiresAt { get; init; }
}
