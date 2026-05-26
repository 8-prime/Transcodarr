using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services.NodeState;
using Transcodarr.Node.Services.Transcoding;
using Transcodarr.Shared.DTOs;
using Transcodarr.Shared;

namespace Transcodarr.Node.Services.Connection;

public class WebsocketConnectionService : BackgroundService
{
    private readonly ConnectionManager _connectionManager;
    private readonly NodeInfoManager _nodeInfoManager;
    private readonly SlotTracker _slotTracker;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly NodeConfiguration _configuration;
    private readonly ILogger<WebsocketConnectionService> _logger;

    public WebsocketConnectionService(
        ConnectionManager connectionManager,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<NodeConfiguration> configuration,
        ILogger<WebsocketConnectionService> logger,
        SlotTracker slotTracker,
        NodeInfoManager nodeInfoManager
    )
    {
        _connectionManager = connectionManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _slotTracker = slotTracker;
        _nodeInfoManager = nodeInfoManager;
        _configuration = configuration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await OpenConnectionAndProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection to core failed");
            }
        }
    }

    private async Task OpenConnectionAndProcessMessagesAsync(CancellationToken stoppingToken)
    {
        var cts = new CancellationTokenSource();
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);
        WebSocket? ws;
        while ((ws = await _connectionManager.ConnectToServerAsync(linked.Token)) is null)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), linked.Token);
        }

        _logger.LogInformation("Connection to core started");

        await _connectionManager.SendAsync(
            new NodeInfoMessage(
                new NodeInfo
                {
                    Name = _configuration.NodeId,
                    EncoderCapabilities = _nodeInfoManager.Capabilities,
                    Slots = _slotTracker.AvailableSlots,
                }
            )
            {
                CorrelationId = Guid.NewGuid(),
            },
            linked.Token
        );

        try
        {
            await Task.WhenAny(
                ProcessMessagesAsync(ws, linked.Token),
                SendHeartBeatsAsync(linked.Token)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection to core failed");
        }

        await cts.CancelAsync();
    }

    private async Task SendHeartBeatsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            _logger.LogDebug("Sending heartbeat");
            await _connectionManager.SendAsync(
                new Heartbeat { CorrelationId = Guid.NewGuid() },
                stoppingToken
            );
        }
    }

    private async Task ProcessMessagesAsync(WebSocket ws, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !ws.CloseStatus.HasValue)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            using var ms = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(buffer, stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                ms.Write(buffer.Array!, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            var message = await JsonSerializer.DeserializeAsync<SocketMessage>(
                ms,
                cancellationToken: stoppingToken
            );
            if (message is null)
            {
                continue;
            }

            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            switch (message)
            {
                case ProbeRequest probeRequest:
                    var fileProbeService =
                        scope.ServiceProvider.GetRequiredService<FileProbeService>();
                    var res = await fileProbeService.ProbeFileAsync(
                        probeRequest.ProbeFilePath,
                        stoppingToken
                    );
                    await _connectionManager.SendAsync(
                        new ProbeResponse(res) { CorrelationId = probeRequest.CorrelationId },
                        stoppingToken
                    );
                    break;
                case TranscodeRequest transcodeRequest:
                    var transcodeService =
                        scope.ServiceProvider.GetRequiredService<TranscodesQueue>();
                    await transcodeService.TranscodeRequests.Writer.WriteAsync(
                        transcodeRequest,
                        stoppingToken
                    );
                    break;
                default:
                    break;
            }
        }
    }
}
