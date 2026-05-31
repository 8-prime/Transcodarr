using System.Net.WebSockets;
using Transcodarr.Shared;

namespace Transcodarr.Core.Common.Models;

public class NodeConnectionInfo : IEquatable<NodeConnectionInfo>
{
    public required string ConnectionId { get; set; }
    public required WebSocket WebSocket { get; set; }
    public Guid SessionId { get; } = Guid.NewGuid();
    public NodeInfo? NodeInfo { get; set; }
    public Dictionary<string, int> FreeSlotsByGroup { get; set; } = new();
    public int TotalFreeSlots => FreeSlotsByGroup.Values.Sum();
    public bool ConnectionIsReady => NodeInfo is not null;

    public bool Equals(NodeConnectionInfo? other) => other?.SessionId == SessionId;

    public override bool Equals(object? obj) => obj is NodeConnectionInfo other && Equals(other);

    public override int GetHashCode() => SessionId.GetHashCode();
}
