using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Transcodarr.Node.Common.Models;
using Transcodarr.Shared.DTOs;
using Transcodearr.Shared;

namespace Transcodarr.Node.Services;

public class WebsocketConnectionService : BackgroundService
{
    private readonly ConnectionManager _connectionManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly NodeConfiguration _configuration;

    public WebsocketConnectionService(ConnectionManager connectionManager, IServiceScopeFactory serviceScopeFactory,
        IOptions<NodeConfiguration> configuration)
    {
        _connectionManager = connectionManager;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration.Value;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ws = await _connectionManager.ConnectToServerAsync(stoppingToken);
        await _connectionManager.SendAsync(new NodeInfoMessage(new NodeInfo
        {
            Name = _configuration.NodeId,
            EncoderCapabilities =
            [
                new EncoderCapability
                {
                    Slots = 1,
                    EncoderName = "hevc_nvenc"
                }
            ]
        })
        {
            CorrelationId = Guid.NewGuid()
        }, stoppingToken);

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
            var message =
                await JsonSerializer.DeserializeAsync<SocketMessage>(ms, cancellationToken: stoppingToken);
            if (message is null)
            {
                continue;
            }

            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            switch (message)
            {
                case ProbeRequest probeRequest:
                    var fileProbeService = scope.ServiceProvider.GetRequiredService<FileProbeService>();
                    var res = await fileProbeService.ProbeFileAsync(probeRequest.ProbeFilePath, stoppingToken);
                    await _connectionManager.SendAsync(new ProbeResponse(res)
                    {
                        CorrelationId = probeRequest.CorrelationId
                    }, stoppingToken);
                    break;
                case TranscodeRequest transcodeRequest:
                    var transcodeService = scope.ServiceProvider.GetRequiredService<TranscodeService>();
                    await transcodeService.RunTranscodeAsync(transcodeRequest.FilePath, transcodeRequest.OutputPath,
                        transcodeRequest.QualitySettings, stoppingToken);
                    break;
            }
        }
    }
}