using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Core.Services.Configuration;
using Transcodarr.Core.Services.MediaFiles;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.Jobs;

public class JobQueueManagerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionManager _connections;
    private readonly WebSocketConnectionService _webSocketConnectionService;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<JobQueueManagerService> _logger;

    public JobQueueManagerService(
        IServiceScopeFactory serviceScopeFactory,
        ConnectionManager connections,
        WebSocketConnectionService webSocketConnectionService,
        ConfigurationService configurationService,
        ILogger<JobQueueManagerService> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _connections = connections;
        _webSocketConnectionService = webSocketConnectionService;
        _configurationService = configurationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            if (!_configurationService.Initialized)
            {
                continue;
            }

            var freeSlots = _connections.GetFreeSlotsForCodec(
                _configurationService.Current.TranscodeVideoCodec
            );
            var now = DateTimeOffset.UtcNow;
            var timedOutJobs = await dbContext
                .TranscodeJobs.Where(j =>
                    j.LeaseExpiresAt <= now && j.Status == TranscodeJobStatus.Active
                )
                .ExecuteUpdateAsync(
                    setter => setter.SetProperty(j => j.Status, TranscodeJobStatus.TimedOut),
                    stoppingToken
                );
            if (timedOutJobs > 0)
                _logger.LogInformation("Marked {TimedOutJobCount} jobs as timed out", timedOutJobs);

            var discoveredFiles = await dbContext
                .MediaFiles.Where(f => f.Status == TranscodeStatus.Discovered)
                .ToListAsync(stoppingToken);
            if (discoveredFiles.Count > 0)
            {
                var fileProbeService = scope.ServiceProvider.GetRequiredService<FileProbeService>();
                _logger.LogInformation(
                    "Retrying probes for {Count} discovered files",
                    discoveredFiles.Count
                );
                foreach (var file in discoveredFiles)
                    await fileProbeService.ProbeFileAsync(file.Path, file.Id, stoppingToken);
            }

            if (freeSlots == 0)
            {
                _logger.LogInformation(
                    "No free slots available on any node, skipping job creation"
                );
                continue;
            }

            var pendingJobs = await dbContext
                .MediaFiles.AsNoTracking()
                .Include(file => file.Jobs)
                .Include(file => file.Metadata)
                .Where(file =>
                    file.Status == TranscodeStatus.Pending
                    && (
                        file.Jobs.Count == 0
                        || file.Jobs.All(job => job.Status != TranscodeJobStatus.Active)
                    )
                )
                .Take(freeSlots)
                .ToListAsync(cancellationToken: stoppingToken);

            _logger.LogInformation(
                "Found {PendingJobCount} pending files eligible for transcoding",
                pendingJobs.Count
            );

            foreach (var pendingRequest in pendingJobs)
            {
                await CreateJob(
                    dbContext,
                    pendingRequest,
                    _configurationService.Current,
                    stoppingToken
                );
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
        if (!_connections.TryGetConnectionForCodec(config.TranscodeVideoCodec, out var connection))
        {
            _logger.LogWarning(
                "No node with a free encoder for {Codec} found while creating job",
                config.TranscodeVideoCodec
            );
            return;
        }

        if (pendingFile.Metadata is null)
        {
            _logger.LogWarning(
                "Skipping job creation for {FilePath}: metadata not available",
                pendingFile.Path
            );
            return;
        }

        var (conn, encoder) = connection.Value;

        _logger.LogInformation(
            "Creating transcode job for {FilePath} on node {NodeId} using encoder {Encoder}",
            pendingFile.Path,
            conn.ConnectionId,
            encoder.EncoderName
        );

        var fileInfo = new FileInfo(pendingFile.Path);
        var outputPath = Path.Join(
            config.TranscodeTempDirectory,
            Path.GetFileNameWithoutExtension(fileInfo.FullName) + FileTypeConstants.TempFileSuffix
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
            pendingFile.Metadata.Duration,
            new TranscodeQualitySettings
            {
                ConstantRateFactor = config.ConstantRateFactor,
                DesiredAudioCodec = config.TranscodeAudioCodec,
                DesiredVideoCodec = config.TranscodeVideoCodec,
                DesiredEncoderPreset = config.TranscodeEncoderPreset,
            },
            encoder.EncoderName
        )
        {
            CorrelationId = Guid.NewGuid(),
        };
        await _webSocketConnectionService.SendFireAndForgetAsync(request, conn, stoppingToken);

        conn.FreeSlotsByGroup[encoder.SlotGroup]--;
    }
}
