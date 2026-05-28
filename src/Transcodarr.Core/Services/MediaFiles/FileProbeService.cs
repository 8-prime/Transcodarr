using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.MediaFiles;

public class FileProbeService
{
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ConnectionManager _connectionManager;
    private readonly ILogger<FileProbeService> _logger;

    public FileProbeService(
        WebSocketConnectionService webSocketConnectionService,
        ConnectionManager connectionManager,
        ILogger<FileProbeService> logger
    )
    {
        _webSocketConnectionService = webSocketConnectionService;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task ProbeFileAsync(
        string path,
        Guid mediaFileId,
        CancellationToken cancellationToken = default
    )
    {
        var conns = _connectionManager.GetConnections();
        if (conns.Count == 0)
        {
            _logger.LogWarning("No connected nodes available, skipping probe for {FilePath}", path);
            return;
        }

        var conn = conns.ElementAt(Random.Shared.Next(conns.Count));
        _logger.LogDebug(
            "Sending probe request for {FilePath} to node {NodeId}",
            path,
            conn.ConnectionId
        );

        var probeRequest = new ProbeRequest(path, mediaFileId) { CorrelationId = Guid.NewGuid() };
        await _webSocketConnectionService.SendFireAndForgetAsync(
            probeRequest,
            conn,
            cancellationToken
        );
    }
}
