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
            _logger.LogInformation(
                "Transcode job {JobId} completed (success={Success}), releasing slot for {Encoder}",
                completed.Result.TranscodeJobId,
                completed.Result.Success,
                encoderName
            );
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
            _logger.LogInformation(
                "Received transcode request for job {JobId} ({FilePath})",
                request.JobLeaseId,
                request.FilePath
            );

            using var scope = _serviceScopeFactory.CreateScope();
            var transcodeService = scope.ServiceProvider.GetRequiredService<TranscodeService>();
            if (!_slotTracker.TryAcquire(request.SpecificEncoder))
            {
                _logger.LogWarning(
                    "No free slot for encoder {Encoder}, rejecting job {JobId}",
                    request.SpecificEncoder,
                    request.JobLeaseId
                );
                await _connectionManager.SendAsync(
                    new TranscodeRejection(request.JobLeaseId) { CorrelationId = Guid.NewGuid() },
                    stoppingToken
                );
                continue;
            }

            _logger.LogInformation(
                "Starting transcode for job {JobId} using encoder {Encoder}",
                request.JobLeaseId,
                request.SpecificEncoder
            );
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
