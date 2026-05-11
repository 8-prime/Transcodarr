namespace Transcodarr.Core.Services;

public class JobQueueManagerService : BackgroundService
{
    private readonly JobQueue _jobQueue;

    public JobQueueManagerService(JobQueue jobQueue)
    {
        _jobQueue = jobQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var transcodeJobRequest in _jobQueue.Reader.ReadAllAsync(stoppingToken))
        {
        }
    }
}