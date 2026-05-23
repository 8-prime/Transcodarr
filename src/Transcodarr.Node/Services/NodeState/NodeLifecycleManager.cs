using Transcodearr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public partial class NodeLifecycleManager(
    CapabilitiesService capabilities,
    SlotTracker slotTracker,
    NodeInfoManager nodeInfoManager,
    ILogger<NodeLifecycleManager> logger)
    : IHostedLifecycleService
{
    public async Task StartingAsync(CancellationToken ct)
    {
        var encoderCapabilities = await capabilities.GetEncodersAsync(ct);
        LogStartingWithEncoders(logger, encoderCapabilities);
        slotTracker.Initialize(encoderCapabilities);
        logger.LogInformation("Initialized transcode slot tracker");
        nodeInfoManager.Initialize(encoderCapabilities);
        logger.LogInformation("Initialized transcode capabilities");
    }

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken ct) => Task.CompletedTask;

    [LoggerMessage(LogLevel.Debug, "Starting with {Encoders}")]
    static partial void LogStartingWithEncoders(ILogger<NodeLifecycleManager> logger, List<EncoderCapability> Encoders);
}
