using System.Net.WebSockets;
using System.Text.Json;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class WebSocketConnectionService
{
    private readonly ConnectionManager _connectionManager;

    public WebSocketConnectionService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task SendFireAndForgetAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : SocketMessage
    {
        var conn = _connectionManager.GetConnections().FirstOrDefault();
        if (conn == null)
        {
            return;
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes<SocketMessage>(message);
        await conn.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
            cancellationToken);
    }

    public async Task<SocketMessage?> SendAsync<TRequest>(TRequest request,
        CancellationToken cancellationToken = default) where TRequest : SocketMessage
    {
        var conn = _connectionManager.GetConnections().FirstOrDefault();
        if (conn == null)
        {
            return null;
        }

        var tcs = new TaskCompletionSource<SocketMessage?>();


        conn.PendingRequests.AddOrUpdate(request.CorrelationId, tcs, (_, _) => tcs);
        var bytes = JsonSerializer.SerializeToUtf8Bytes<SocketMessage>(request);
        await conn.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
            cancellationToken);
        var result = await tcs.Task;
        conn.PendingRequests.TryRemove(request.CorrelationId, out _);
        return result;
    }
}