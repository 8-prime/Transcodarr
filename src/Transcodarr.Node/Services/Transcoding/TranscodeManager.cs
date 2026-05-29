using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services.Connection;
using Transcodarr.Node.Services.NodeState;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Node.Services.Transcoding;

public class TranscodeManager : BackgroundService
{
    private readonly ILogger<TranscodeManager> _logger;
    private readonly ConnectionManager _connectionManager;
    private readonly TranscodesQueue _transcodeRequests;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SlotTracker _slotTracker;
    private readonly List<Task<TranscodeResponse>> _transcodeJobs = [];

    public TranscodeManager(
        ConnectionManager connectionManager,
        TranscodesQueue queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TranscodeManager> logger,
        SlotTracker slotTracker
    )
    {
        _connectionManager = connectionManager;
        _transcodeRequests = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _slotTracker = slotTracker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(ProcessTranscodes(stoppingToken), AwaitCompletions(stoppingToken));
    }

    private async Task AwaitCompletions(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_transcodeJobs.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            var completed = await Task.WhenAny(_transcodeJobs);
            var encoderName = completed.Result.EncoderSettingsSnapshot.EncoderName;
            await _connectionManager.SendAsync(completed.Result, stoppingToken);
            await _connectionManager.SendAsync(
                new IncrementSlotsMessage(encoderName) { CorrelationId = Guid.NewGuid() },
                stoppingToken
            );
            _transcodeJobs.Remove(completed);
            _slotTracker.Release(encoderName);
        }
    }

    private async Task ProcessTranscodes(CancellationToken stoppingToken)
    {
        await foreach (
            var request in _transcodeRequests.TranscodeRequests.Reader.ReadAllAsync(stoppingToken)
        )
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var transcodeService = scope.ServiceProvider.GetRequiredService<TranscodeService>();
            if (!_slotTracker.TryAcquire(request.SpecificEncoder))
            {
                await _connectionManager.SendAsync(
                    new TranscodeRejection(request.JobLeaseId) { CorrelationId = Guid.NewGuid() },
                    stoppingToken
                );
                continue;
            }

            _transcodeJobs.Add(
                transcodeService.RunTranscodeAsync(
                    request.JobLeaseId,
                    request.FilePath,
                    request.OutputPath,
                    request.TotalDuration,
                    request.QualitySettings,
                    request.SpecificEncoder,
                    stoppingToken
                )
            );
        }
    }
}
