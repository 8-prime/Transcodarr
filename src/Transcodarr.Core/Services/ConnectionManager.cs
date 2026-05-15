using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Transcodarr.Core.Common.Models;

namespace Transcodarr.Core.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, NodeConnectionInfo> _connections = [];

    public NodeConnectionInfo AddConnection(WebSocket socket, string nodeId)
    {
        var newConnection = new NodeConnectionInfo
        {
            ConnectionId = nodeId,
            WebSocket = socket
        };
        _connections.AddOrUpdate(nodeId, newConnection, (_, _) => newConnection);
        return newConnection;
    }

    public void CloseConnection(string nodeId)
    {
        _connections.TryRemove(nodeId, out _);
    }

    public ICollection<NodeConnectionInfo> GetConnections()
    {
        return _connections.Values;
    }

    public bool TryGetConnection(string nodeId, [NotNullWhen(true)] out NodeConnectionInfo? connection)
    {
        return _connections.TryGetValue(nodeId, out connection);
    }

    public int GetTotalFreeSlots()
    {
        return _connections.Values.Sum(x => x.FreeSlots);
    }
}