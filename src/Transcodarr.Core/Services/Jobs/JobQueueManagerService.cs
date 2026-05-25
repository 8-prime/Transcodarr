using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Core.Services.Jobs;

public class JobQueueManagerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionManager _connections;
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ILogger<JobQueueManagerService> _logger;

    public JobQueueManagerService(
        IServiceScopeFactory serviceScopeFactory,
        ConnectionManager connections,
        WebSocketConnectionService webSocketConnectionService,
        ILogger<JobQueueManagerService> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _connections = connections;
        _webSocketConnectionService = webSocketConnectionService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var config = await dbContext.AppConfigurations.FirstOrDefaultAsync(
                cancellationToken: stoppingToken
            );
            if (config == null)
            {
                continue;
            }

            var freeSlots = _connections.GetTotalFreeSlots();

            var timedOutJobs = await dbContext
                .TranscodeJobs.Where(j =>
                    j.LeaseExpiresAt <= DateTime.UtcNow && j.Status == TranscodeJobStatus.Active
                )
                .ExecuteUpdateAsync(
                    setter =>
                        setter
                            .SetProperty(j => j.LeaseExpiresAt, (DateTimeOffset?)null)
                            .SetProperty(j => j.Status, TranscodeJobStatus.TimedOut),
                    stoppingToken
                );
            _logger.LogInformation("Found {TimeoutOutJobsCound} timed out jobs", timedOutJobs);

            var pendingJobs = await dbContext
                .MediaFiles.AsNoTracking()
                .Include(request => request.Jobs)
                .Where(request =>
                    (request.Status == TranscodeStatus.Pending) && request.Jobs.Count == 0
                    || request.Jobs.All(job => job.Status != TranscodeJobStatus.Active)
                )
                .Take(freeSlots)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var pendingRequest in pendingJobs)
            {
                await CreateJob(dbContext, pendingRequest, config, stoppingToken);
                freeSlots--;
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task CreateJob(
        TranscodarrDbContext dbContext,
        MediaFileEntity pendingFile,
        AppConfigurationEntity config,
        CancellationToken stoppingToken
    )
    {
        var conn = _connections.GetConnections().FirstOrDefault(c => c.FreeSlots > 0);
        if (conn is null)
        {
            return;
        }

        var fileInfo = new FileInfo(pendingFile.Path);
        var outputPath = Path.Join(
            config.TranscodeTempDirectory,
            Path.GetFileName(fileInfo.FullName),
            FileTypeConstants.TempFileSuffix
        );

        var newJob = new TranscodeJobEntity
        {
            Id = Guid.NewGuid(),
            NodeId = conn.ConnectionId,
            OutputPath = outputPath,
            MediaFile = pendingFile,
            Status = TranscodeJobStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            LeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(config.JobExpirationInMinutes),
            AudioCodec = config.TranscodeAudioCodec,
            VideoCodec = config.TranscodeVideoCodec,
            EncoderPreset = config.TranscodeEncoderPreset,
            ConstantRateFactor = config.ConstantRateFactor,
        };

        dbContext.TranscodeJobs.Add(newJob);

        var request = new TranscodeRequest(
            pendingFile.Path,
            outputPath,
            newJob.Id,
            new TranscodeQualitySettings
            {
                ConstantRateFactor = config.ConstantRateFactor,
                DesiredAudioCodec = config.TranscodeAudioCodec,
                DesiredVideoCodec = config.TranscodeVideoCodec,
                DesiredEncoderPreset = config.TranscodeEncoderPreset,
            }
        )
        {
            CorrelationId = Guid.NewGuid(),
        };
        await _webSocketConnectionService.SendFireAndForgetAsync(request, conn, stoppingToken);

        conn.FreeSlots--;
    }
}
