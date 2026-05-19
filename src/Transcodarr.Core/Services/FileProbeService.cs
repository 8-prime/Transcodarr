using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class FileProbeService
{
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ConnectionManager _connectionManager;

    public FileProbeService(WebSocketConnectionService webSocketConnectionService, ConnectionManager connectionManager)
    {
        _webSocketConnectionService = webSocketConnectionService;
        _connectionManager = connectionManager;
    }

    public async Task<FileProbeResult?> ProbeFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var conn = _connectionManager.GetConnections().FirstOrDefault();
        if (conn is null)
        {
            return null;
        }

        var probeRequest = new ProbeRequest(path)
        {
            CorrelationId = Guid.NewGuid()
        };
        var result = await _webSocketConnectionService.SendAsync(probeRequest, conn, cancellationToken);
        return result is not ProbeResponse response ? null : response.Result;
    }
}