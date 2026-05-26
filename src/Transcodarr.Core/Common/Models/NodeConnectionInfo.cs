using System.Net.WebSockets;
using Transcodarr.Shared;

namespace Transcodarr.Core.Common.Models;

public class NodeConnectionInfo
{
    public required string ConnectionId { get; set; }
    public required WebSocket WebSocket { get; set; }
    public NodeInfo? NodeInfo { get; set; }
    public int FreeSlots { get; set; }
    public bool ConnectionIsReady => NodeInfo is not null;
}
