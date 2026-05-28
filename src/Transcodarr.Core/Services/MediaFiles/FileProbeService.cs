using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.MediaFiles;

public class FileProbeService
{
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ConnectionManager _connectionManager;
    private readonly ProbeManagerService _probeManager;
    private readonly ILogger<FileProbeService> _logger;

    public FileProbeService(
        WebSocketConnectionService webSocketConnectionService,
        ConnectionManager connectionManager,
        ProbeManagerService probeManager,
        ILogger<FileProbeService> logger
    )
    {
        _webSocketConnectionService = webSocketConnectionService;
        _connectionManager = connectionManager;
        _probeManager = probeManager;
        _logger = logger;
    }

    public async Task ProbeFileAsync(
        string path,
        Guid mediaFileId,
        CancellationToken cancellationToken = default
    )
    {
        if (_probeManager.IsInFlight(mediaFileId))
        {
            _logger.LogDebug("Probe already in-flight for {FileId}, skipping", mediaFileId);
            return;
        }

        var conns = _connectionManager.GetConnections();
        if (conns.Count == 0)
        {
            _logger.LogWarning("No connected nodes available, skipping probe for {FilePath}", path);
            return;
        }

        var conn = conns.ElementAt(Random.Shared.Next(conns.Count));

        if (!_probeManager.TryStart(mediaFileId, conn.ConnectionId))
        {
            _logger.LogDebug("Probe already in-flight for {FileId} (race), skipping", mediaFileId);
            return;
        }

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
