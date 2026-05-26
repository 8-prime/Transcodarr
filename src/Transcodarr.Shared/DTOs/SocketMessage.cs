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
public abstract record SocketMessage
{
    public required Guid CorrelationId { get; init; }
}

public record Heartbeat : SocketMessage;

public record ProbeRequest(string ProbeFilePath, Guid MediaFileId) : SocketMessage;

public record ProbeResponse(FileProbeResult? Result) : SocketMessage;

public record NodeInfoMessage(NodeInfo Info) : SocketMessage;

public record TranscodeRequest(
    string FilePath,
    string OutputPath,
    Guid JobLeaseId,
    TranscodeQualitySettings QualitySettings
) : SocketMessage;

public record TranscodeRejection(Guid JobLeaseId) : SocketMessage;

public record IncrementSlotsMessage : SocketMessage;

public record TranscodeResponse(
    Guid TranscodeJobId,
    bool Success,
    TranscoderSnapshot EncoderSettingsSnapshot,
    long OutputSizeBytes,
    double VMafScore
) : SocketMessage;

public record TranscodeProgress(double ProgressPercent) : SocketMessage;
