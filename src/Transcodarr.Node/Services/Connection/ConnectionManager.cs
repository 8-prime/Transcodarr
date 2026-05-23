using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Transcodarr.Node.Common.Models;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Connection;

public class ConnectionManager
{
    private readonly NodeConfiguration _configuration;
    private WebSocket? _webSocket;
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(IOptions<NodeConfiguration> configuration, ILogger<ConnectionManager> logger)
    {
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<WebSocket?> ConnectToServerAsync(CancellationToken stoppingToken)
    {
        var ws = new ClientWebSocket();
        var endpoint = new Uri($"{_configuration.CoreUrl}/connections/{_configuration.NodeId}");

        try
        {
            await ws.ConnectAsync(endpoint, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to server");
            return null;
        }

        _webSocket = ws;
        return _webSocket;
    }

    public async Task SendAsync<T>(T message, CancellationToken stoppingToken) where T : SocketMessage
    {
        if (_webSocket == null)
        {
            return;
        }

        Memory<byte> payLoad = JsonSerializer.SerializeToUtf8Bytes<SocketMessage>(message);
        await _webSocket.SendAsync(payLoad, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage,
            stoppingToken);
    }
}
