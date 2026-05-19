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

    public async Task<SocketMessage?> SendAsync<TRequest>(TRequest request, NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken = default) where TRequest : SocketMessage
    {
        var tcs = new TaskCompletionSource<SocketMessage?>();


        nodeConnectionInfo.PendingRequests.AddOrUpdate(request.CorrelationId, tcs, (_, _) => tcs);
        var bytes = JsonSerializer.SerializeToUtf8Bytes<SocketMessage>(request);
        await nodeConnectionInfo.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
            cancellationToken);
        var result = await tcs.Task;
        nodeConnectionInfo.PendingRequests.TryRemove(request.CorrelationId, out _);
        return result;
    }
}