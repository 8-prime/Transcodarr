using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Core.Services.Configuration;
using Transcodarr.Core.Services.MediaFiles;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.Connection;

public partial class MessageHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MessageHandler> logger,
        ConfigurationService configurationService
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task ProcessMessageAsync(
        SocketMessage message,
        NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken
    )
    {
        switch (message)
        {
            case NodeInfoMessage nodeInfoMessage:
                nodeConnectionInfo.NodeInfo = nodeInfoMessage.Info;
                break;
            case TranscodeResponse transcodeResponse:
                await HandleTranscodeResponse(transcodeResponse, cancellationToken);
                break;
            case Heartbeat _:
                await HandleHeartbeat(nodeConnectionInfo, cancellationToken);
                break;
            case IncrementSlotsMessage _:
                nodeConnectionInfo.FreeSlots++;
                break;
            case TranscodeProgress progress:
                await HandleProgress(progress, cancellationToken);
                break;
            case ProbeResponse response:
                await HandleProbeResponse(response, cancellationToken);
                break;
            default:
                break;
        }
    }

    private async Task HandleProbeResponse(
        ProbeResponse probeResponse,
        CancellationToken cancellationToken
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
        var file = await context.MediaFiles.FirstOrDefaultAsync(
            f => f.Id == probeResponse.MediaFileId,
            cancellationToken
        );

        if (file is null || !File.Exists(file.Path))
        {
            return;
        }

        if (probeResponse.Result is null)
        {
            var fileProbeService = scope.ServiceProvider.GetRequiredService<FileProbeService>();
            await fileProbeService.ProbeFileAsync(file.Path, file.Id, cancellationToken);
            return;
        }

        file.Metadata = new MediaFileMetadataEntity
        {
            Id = Guid.NewGuid(),
            AudioStreams = probeResponse.Result.AudioStreams,
            VideoCodec = probeResponse.Result.VideoCodec,
            Duration = probeResponse.Result.Duration,
            BitRate = probeResponse.Result.Bitrate,
            Height = probeResponse.Result.Height,
            Width = probeResponse.Result.Width,
            IsHdr = probeResponse.Result.IsHdr,
            FileSizeBytes = new FileInfo(file.Path).Length,
        };

        file.Status = TranscodeStatus.Pending;

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleProgress(
        TranscodeProgress progress,
        CancellationToken cancellationToken
    )
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();

        await context
            .TranscodeJobs.Where(j => j.Id == progress.TranscodeJobId)
            .ExecuteUpdateAsync(
                properties =>
                    properties
                        .SetProperty(j => j.LeaseExpiresAt, DateTimeOffset.UtcNow.AddMinutes(30))
                        .SetProperty(j => j.Progress, progress.ProgressPercent),
                cancellationToken: cancellationToken
            );
    }

    private async Task HandleHeartbeat(
        NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken
    )
    {
        LogReceivedHeartbeatFromNodeId(nodeConnectionInfo.ConnectionId);
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var wsConnectionService =
            scope.ServiceProvider.GetRequiredService<WebSocketConnectionService>();
        await wsConnectionService.SendFireAndForgetAsync(
            new Heartbeat { CorrelationId = Guid.NewGuid() },
            nodeConnectionInfo,
            cancellationToken
        );
    }

    private async Task HandleTranscodeResponse(
        TranscodeResponse transcodeResponse,
        CancellationToken cancellationToken
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();

        var job = context
            .TranscodeJobs.Include(r => r.MediaFile)
            .FirstOrDefault(j => j.Id == transcodeResponse.TranscodeJobId);
        if (job == null)
        {
            return;
        }

        if (!transcodeResponse.Success)
        {
            //check attempt and requeue
            job.Status = TranscodeJobStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (_configurationService.Current.AutoApplyTranscode)
        {
            if (!File.Exists(job.OutputPath))
            {
                //check attempt and requeue
                return;
            }

            File.Move(job.OutputPath, job.MediaFile.Path, true);
            var fileInfo = new FileInfo(job.MediaFile.Path);
            job.Status = TranscodeJobStatus.Completed;
            job.MediaFile.FileModifiedAt = fileInfo.LastWriteTimeUtc;
            job.MediaFile.Status = TranscodeStatus.Completed;
            context.TranscodeResults.Add(
                new TranscodeResultEntity()
                {
                    ApprovalState = ApprovalState.AutoApproved,
                    CompletedAt = DateTimeOffset.UtcNow,
                    FileSizeBytes = transcodeResponse.OutputSizeBytes,
                    TranscodeJob = job,
                    VmafScore = 0,
                }
            );
        }
        else
        {
            job.Status = TranscodeJobStatus.Completed;
            context.TranscodeResults.Add(
                new TranscodeResultEntity()
                {
                    ApprovalState = ApprovalState.Pending,
                    CompletedAt = DateTimeOffset.UtcNow,
                    FileSizeBytes = transcodeResponse.OutputSizeBytes,
                    TranscodeJob = job,
                    VmafScore = 0,
                }
            );
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "Received heartbeat from {nodeId}")]
    partial void LogReceivedHeartbeatFromNodeId(string nodeId);
}
