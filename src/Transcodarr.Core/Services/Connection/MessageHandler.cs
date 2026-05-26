using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Core.Services.Configuration;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services.Connection;

public partial class MessageHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConfigurationService _configurationService;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceScopeFactory serviceScopeFactory, ILogger<MessageHandler> logger,
        ConfigurationService configurationService)
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
                await HandleProbeResponse(response.Result, cancellationToken);
                break;
            default:
                break;
        }
    }

    private async Task HandleProbeResponse(FileProbeResult fileProbeResult, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
        var file = await context.MediaFiles.FirstOrDefaultAsync(f => f.Id == fileProbeResult.MediaFileId,
            cancellationToken);

        file?.Metadata = new MediaFileMetadataEntity
        {
            Id = Guid.NewGuid(),
            AudioStreams = fileProbeResult.AudioStreams,
            VideoCodec = fileProbeResult.VideoCodec,
            Duration = fileProbeResult.Duration,
            BitRate = fileProbeResult.Bitrate,
            FileSizeBytes = new FileInfo(file.Path).Length,
            Height = fileProbeResult.Height,
            Width = fileProbeResult.Width,
            IsHdr = fileProbeResult.IsHdr,
        };

        file?.Status = TranscodeStatus.Pending;

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

        if (!transcodeResponse.Success)
        {
            //check attempt and requeue
            return;
        }

        var jobLease = context
            .TranscodeJobs.Include(r => r.MediaFile)
            .FirstOrDefault(j => j.Id == transcodeResponse.TranscodeJobId);
        if (jobLease == null)
        {
            throw new ApplicationException($"Lease {transcodeResponse.TranscodeJobId} not found");
        }

        if (_configurationService.Current.AutoApplyTranscode)
        {
            if (!File.Exists(jobLease.OutputPath))
            {
                //check attempt and requeue
                return;
            }

            File.Move(jobLease.OutputPath, jobLease.MediaFile.Path, true);
            var fileInfo = new FileInfo(jobLease.MediaFile.Path);
            jobLease.Status = TranscodeJobStatus.Completed;
            jobLease.MediaFile.FileModifiedAt = fileInfo.LastWriteTimeUtc;

            context.TranscodeResults.Add(
                new TranscodeResultEntity()
                {
                    ApprovalState = ApprovalState.AutoApproved,
                    CompletedAt = DateTimeOffset.UtcNow,
                    FileSizeBytes = transcodeResponse.OutputSizeBytes,
                    TranscodeJob = jobLease,
                    VmafScore = 0,
                }
            );
        }
        else
        {
            context.TranscodeResults.Add(
                new TranscodeResultEntity()
                {
                    ApprovalState = ApprovalState.Pending,
                    CompletedAt = DateTimeOffset.UtcNow,
                    FileSizeBytes = transcodeResponse.OutputSizeBytes,
                    TranscodeJob = jobLease,
                    VmafScore = 0,
                }
            );
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "Received heartbeat from {nodeId}")]
    partial void LogReceivedHeartbeatFromNodeId(string nodeId);
}