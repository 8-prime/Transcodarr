using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Services.MediaFiles;
using Transcodarr.Shared;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, NodeConnectionInfo> _connections = [];
    private readonly ProbeManagerService _probeManager;

    public ConnectionManager(ProbeManagerService probeManager)
    {
        _probeManager = probeManager;
    }

    public NodeConnectionInfo AddConnection(WebSocket socket, string nodeId)
    {
        var newConnection = new NodeConnectionInfo { ConnectionId = nodeId, WebSocket = socket };
        _connections.AddOrUpdate(nodeId, newConnection, (_, _) => newConnection);
        return newConnection;
    }

    public void CloseConnection(string nodeId)
    {
        _connections.TryRemove(nodeId, out _);
        _probeManager.ClearNode(nodeId);
    }

    public ICollection<NodeConnectionInfo> GetConnections()
    {
        return _connections.Values;
    }

    public bool TryGetConnection(
        string nodeId,
        [NotNullWhen(true)] out NodeConnectionInfo? connection
    )
    {
        return _connections.TryGetValue(nodeId, out connection);
    }

    public bool TryGetConnectionForCodec(
        VideoCodec codec,
        [NotNullWhen(true)] out (NodeConnectionInfo, EncoderCapability)? connection
    )
    {
        foreach (var nodeConnectionInfo in _connections.Values)
        {
            if (nodeConnectionInfo.NodeInfo is not { } nodeInfo)
            {
                continue;
            }

            var matchingEncoders = nodeInfo.EncoderCapabilities.Where(e => e.CodecType == codec);
            foreach (var encoder in matchingEncoders)
            {
                if (nodeInfo.SlotGroupCapacities.GetValueOrDefault(encoder.SlotGroup) <= 0)
                {
                    continue;
                }

                connection = (nodeConnectionInfo, encoder);
                return true;
            }
        }

        connection = null;
        return false;
    }

    public int GetFreeSlotsForCodec(VideoCodec codec)
    {
        var total = 0;
        foreach (var nodeInfo in _connections.Values.Select(n => n.NodeInfo).OfType<NodeInfo>())
        {
            var matchingEncoders = nodeInfo
                .EncoderCapabilities.Where(e => e.CodecType == codec)
                .Select(e => e.SlotGroup);
            total += matchingEncoders.Sum(e => nodeInfo.SlotGroupCapacities.GetValueOrDefault(e));
        }

        return total;
    }
}
