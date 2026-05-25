using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.MediaFiles;

public class FileProbeService
{
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ConnectionManager _connectionManager;

    public FileProbeService(WebSocketConnectionService webSocketConnectionService, ConnectionManager connectionManager)
    {
        _webSocketConnectionService = webSocketConnectionService;
        _connectionManager = connectionManager;
    }

    public async Task ProbeFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var conns = _connectionManager.GetConnections();
        if (conns.Count == 0) return;
        var conn = conns.ElementAt(Random.Shared.Next(conns.Count - 1));

        var probeRequest = new ProbeRequest(path)
        {
            CorrelationId = Guid.NewGuid()
        };
        await _webSocketConnectionService.SendFireAndForgetAsync(probeRequest, conn, cancellationToken);
    }
}