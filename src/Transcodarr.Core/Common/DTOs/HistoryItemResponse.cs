using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Common.DTOs;

public class HistoryItemResponse
{
    public required Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string LibraryName { get; init; }
    public required string EncoderUsed { get; init; }
    public required VideoCodec VideoCodec { get; init; }
    public required AudioCodec AudioCodec { get; init; }
    public required int Crf { get; init; }
    public required double VmafScore { get; init; }
    public required long InputSizeBytes { get; init; }
    public required long OutputSizeBytes { get; init; }
    public required double DurationSec { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    public required ApprovalState ApprovalState { get; init; }
}
