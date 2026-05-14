using System.Text.Json.Serialization;
using Transcodearr.Shared;

namespace Transcodarr.Shared.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ProbeResponse), nameof(ProbeResponse))]
[JsonDerivedType(typeof(ProbeRequest), nameof(ProbeRequest))]
[JsonDerivedType(typeof(NodeInfoMessage), nameof(NodeInfoMessage))]
public abstract record SocketMessage
{
    public required Guid CorrelationId { get; init; }
}

public record ProbeRequest(string ProbeFilePath) : SocketMessage;

public record ProbeResponse(FileProbeResult Result) : SocketMessage;

public record NodeInfoMessage(NodeInfo Info) : SocketMessage;

public record TranscodeRequest(string FilePath, Guid FileInfoId) : SocketMessage;

public record TranscodeResponse(Guid FileInfoId, Guid JobLeaseId, bool Success, string EncoderSettingsSnapshot, long OutputSizeBytes, double VMafScore) : SocketMessage;