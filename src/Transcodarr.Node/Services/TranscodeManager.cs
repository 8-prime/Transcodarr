using Transcodarr.Node.Services;

namespace Transcodarr.Core.Services;

public class TranscodeManager : BackgroundService
{
    private readonly ConnectionManager _connectionManager;
    private readonly TranscodesQueue _transcodeRequests;
    private readonly SemaphoreSlim _semaphoreSlim;

    public TranscodeManager(ConnectionManager connectionManager, TranscodesQueue queue)
    {
        _connectionManager = connectionManager;
        _transcodeRequests = queue;
        //TODO: determine max concurrent streams supported on hardware level
        _semaphoreSlim = new SemaphoreSlim(1, 1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessTranscodes(CancellationToken stoppingToken)
    {
        await _semaphoreSlim.WaitAsync(stoppingToken);
        try
        {
            await foreach (var request in _transcodeRequests.TranscodeRequests.Reader.ReadAllAsync(stoppingToken))
            {
                
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}