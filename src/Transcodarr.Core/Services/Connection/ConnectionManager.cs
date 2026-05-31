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

    public void CloseConnection(NodeConnectionInfo connection)
    {
        _connections.TryRemove(
            new KeyValuePair<string, NodeConnectionInfo>(connection.ConnectionId, connection)
        );
        _probeManager.ClearNode(connection.ConnectionId);
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
        List<(NodeConnectionInfo, EncoderCapability)> matches = [];
        foreach (var nodeConnectionInfo in _connections.Values)
        {
            if (nodeConnectionInfo.NodeInfo is not { } nodeInfo)
            {
                continue;
            }

            var matchingEncoders = nodeInfo.EncoderCapabilities.Where(e => e.CodecType == codec);
            foreach (var encoder in matchingEncoders)
            {
                if (nodeConnectionInfo.FreeSlotsByGroup.GetValueOrDefault(encoder.SlotGroup) <= 0)
                {
                    continue;
                }

                matches.Add((nodeConnectionInfo, encoder));
            }
        }

        if (matches.Count == 0)
        {
            connection = null;
            return false;
        }

        connection = matches.All(m => m.Item2.SlotGroup == EncoderGroup.Software)
            ? matches.FirstOrDefault()
            : matches.FirstOrDefault(m => m.Item2.SlotGroup != EncoderGroup.Software);

        return true;
    }

    public int GetFreeSlotsForCodec(VideoCodec codec)
    {
        var total = 0;
        foreach (var nodeConnectionInfo in _connections.Values)
        {
            if (nodeConnectionInfo.NodeInfo is not { } nodeInfo)
                continue;

            var matchingEncoders = nodeInfo
                .EncoderCapabilities.Where(e => e.CodecType == codec)
                .Select(e => e.SlotGroup);
            total += matchingEncoders.Sum(e =>
                nodeConnectionInfo.FreeSlotsByGroup.GetValueOrDefault(e)
            );
        }

        return total;
    }
}