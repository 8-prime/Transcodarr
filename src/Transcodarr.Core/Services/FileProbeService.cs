using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class FileProbeService
{
    private readonly WebSocketConnectionService _webSocketConnectionService;

    public FileProbeService(WebSocketConnectionService webSocketConnectionService)
    {
        _webSocketConnectionService = webSocketConnectionService;
    }

    public async Task<FileProbeResult?> ProbeFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var probeRequest = new ProbeRequest(path)
        {
            CorrelationId = Guid.NewGuid()
        };
        var result = await _webSocketConnectionService.SendAsync(probeRequest, cancellationToken);
        return result is not ProbeResponse response ? null : response.Result;
    }
}