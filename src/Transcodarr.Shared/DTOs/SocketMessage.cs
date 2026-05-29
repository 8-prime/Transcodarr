using System.Text.Json.Serialization;
using Transcodarr.Shared;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Shared.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ProbeResponse), nameof(ProbeResponse))]
[JsonDerivedType(typeof(ProbeRequest), nameof(ProbeRequest))]
[JsonDerivedType(typeof(NodeInfoMessage), nameof(NodeInfoMessage))]
[JsonDerivedType(typeof(Heartbeat), nameof(Heartbeat))]
[JsonDerivedType(typeof(TranscodeProgress), nameof(TranscodeProgress))]
[JsonDerivedType(typeof(TranscodeRequest), nameof(TranscodeRequest))]
[JsonDerivedType(typeof(TranscodeResponse), nameof(TranscodeResponse))]
[JsonDerivedType(typeof(TranscodeRejection), nameof(TranscodeRejection))]
[JsonDerivedType(typeof(IncrementSlotsMessage), nameof(IncrementSlotsMessage))]
public abstract record SocketMessage
{
    public required Guid CorrelationId { get; init; }
}

public record Heartbeat : SocketMessage;

public record ProbeRequest(string ProbeFilePath, Guid MediaFileId) : SocketMessage;

public record ProbeResponse(Guid MediaFileId, FileProbeResult? Result) : SocketMessage;

public record NodeInfoMessage(NodeInfo Info) : SocketMessage;

public record TranscodeRequest(
    string FilePath,
    string OutputPath,
    Guid JobLeaseId,
    TimeSpan TotalDuration,
    TranscodeQualitySettings QualitySettings,
    string SpecificEncoder
) : SocketMessage;

public record TranscodeRejection(Guid JobLeaseId) : SocketMessage;

public record IncrementSlotsMessage(string EncoderName) : SocketMessage;

public record TranscodeResponse(
    Guid TranscodeJobId,
    bool Success,
    TranscoderSnapshot EncoderSettingsSnapshot,
    long OutputSizeBytes,
    double VMafScore
) : SocketMessage;

public record TranscodeProgress(double ProgressPercent, Guid TranscodeJobId) : SocketMessage;
