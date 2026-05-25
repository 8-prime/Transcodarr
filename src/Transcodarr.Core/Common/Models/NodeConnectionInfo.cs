using System.Collections.Concurrent;
using System.Net.WebSockets;
using Transcodarr.Shared.DTOs;
using Transcodearr.Shared;

namespace Transcodarr.Core.Common.Models;

public class NodeConnectionInfo
{
    public required string ConnectionId { get; set; }
    public required WebSocket WebSocket { get; set; }
    public NodeInfo? NodeInfo { get; set; }
    public int FreeSlots { get; set; }
    public ConcurrentDictionary<
        Guid,
        TaskCompletionSource<SocketMessage?>
    > PendingRequests { get; set; } = [];
    public bool ConnectionIsReady => NodeInfo is not null;
}
