using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Services;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Endpoints;

public static class ConnectionEndpoints
{
    public static IEndpointRouteBuilder MapConnection(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("connections");
        group.MapGet("", GetConnections);
        group.Map("{id}", MapAddConnection);
        return endpoints;
    }


    private static Ok<ICollection<NodeConnectionInfo>> GetConnections(
        [FromServices] ConnectionManager connectionManager)
    {
        return TypedResults.Ok(connectionManager.GetConnections());
    }

    private static async Task<Results<BadRequest, Ok>> MapAddConnection(string id, HttpContext context,
        [FromServices] ConnectionManager connectionManager, [FromServices] MessageHandler messageHandler,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                return TypedResults.BadRequest();
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionInfo = connectionManager.AddConnection(socket, id);
            await ReadMessages(socket, connectionInfo, messageHandler, cancellationToken);
            connectionManager.CloseConnection(id);
            return TypedResults.Ok();
        }
        catch (Exception)
        {
            connectionManager.CloseConnection(id);
        }

        return TypedResults.Ok();
    }

    private static async Task ReadMessages(WebSocket socket, NodeConnectionInfo info, MessageHandler messageHandler,
        CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                ms.Write(buffer.Array!, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            var message =
                await JsonSerializer.DeserializeAsync<SocketMessage>(ms, cancellationToken: cancellationToken);
            if (message is null) continue;
            if (info.PendingRequests.TryGetValue(message.CorrelationId, out var tcs))
            {
                tcs.SetResult(message);
            }
            else
            {
                await messageHandler.ProcessMessageAsync(message, info, cancellationToken);
            }
        }
    }
}