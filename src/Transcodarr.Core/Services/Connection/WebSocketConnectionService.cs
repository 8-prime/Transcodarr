using System.Net.WebSockets;
using System.Text.Json;
using Transcodarr.Core.Common.Models;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class WebSocketConnectionService
{
    public async Task SendFireAndForgetAsync<T>(T message, NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken = default)
        where T : SocketMessage
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes<SocketMessage>(message);
        await nodeConnectionInfo.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
            cancellationToken);
    }
}