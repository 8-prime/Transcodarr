using Transcodarr.Node.Services.Connection;
using Transcodarr.Node.Services.NodeState;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class TranscodeManager : BackgroundService
{
    private readonly ILogger<TranscodeManager> _logger;
    private readonly ConnectionManager _connectionManager;
    private readonly TranscodesQueue _transcodeRequests;
    private readonly TranscodeService _transcodeService;
    private readonly SlotTracker _slotTracker;
    private readonly List<Task<TranscodeResponse>> _transcodeJos = [];

    public TranscodeManager(ConnectionManager connectionManager, TranscodesQueue queue,
        ILogger<TranscodeManager> logger, SlotTracker slotTracker, TranscodeService transcodeService)
    {
        _connectionManager = connectionManager;
        _transcodeRequests = queue;
        _logger = logger;
        _slotTracker = slotTracker;
        _transcodeService = transcodeService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(ProcessTranscodes(stoppingToken), AwaitCompletions(stoppingToken));
    }

    private async Task AwaitCompletions(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_transcodeJos.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            var completed = await Task.WhenAny(_transcodeJos);
            await _connectionManager.SendAsync(completed.Result, stoppingToken);
            await _connectionManager.SendAsync(new IncrementSlotsMessage
            {
                CorrelationId = Guid.NewGuid(),
            }, stoppingToken);
            _transcodeJos.Remove(completed);
            _slotTracker.Release(completed.Result.EncoderSettingsSnapshot.EncoderName);
        }
    }

    private async Task ProcessTranscodes(CancellationToken stoppingToken)
    {
        await foreach (var request in _transcodeRequests.TranscodeRequests.Reader.ReadAllAsync(
                           stoppingToken))
        {
            var firstFreeEncoder = _slotTracker.EncodersWithCapacity.FirstOrDefault();
            if (firstFreeEncoder == null || !_slotTracker.TryAcquire(firstFreeEncoder))
            {
                await _connectionManager.SendAsync(new TranscodeRejection(request.JobLeaseId)
                {
                    CorrelationId = Guid.NewGuid(),
                }, stoppingToken);
                continue;
            }

            _transcodeJos.Add(_transcodeService.RunTranscodeAsync(request.JobLeaseId, request.FilePath,
                request.OutputPath, request.QualitySettings, firstFreeEncoder, stoppingToken));
        }
    }
}
