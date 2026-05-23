using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public partial class MessageHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceScopeFactory serviceScopeFactory, ILogger<MessageHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task ProcessMessageAsync(SocketMessage message, NodeConnectionInfo nodeConnectionInfo,
        CancellationToken cancellationToken)
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
            default:
                break;
        }
    }

    private async Task HandleHeartbeat(NodeConnectionInfo nodeConnectionInfo, CancellationToken cancellationToken)
    {
        LogReceivedHeartbeatFromNodeId(nodeConnectionInfo.ConnectionId);
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var wsConnectionService = scope.ServiceProvider.GetRequiredService<WebSocketConnectionService>();
        await wsConnectionService.SendFireAndForgetAsync(new Heartbeat
        {
            CorrelationId = Guid.NewGuid(),
        }, nodeConnectionInfo, cancellationToken);
    }

    private async Task HandleTranscodeResponse(TranscodeResponse transcodeResponse, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
        var settings = await context.AppConfigurations.AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (settings == null)
        {
            throw new ApplicationException($"No settings found for {nameof(TranscodeResponse)}");
        }

        if (!transcodeResponse.Success)
        {
            //check attempt and requeue
            return;
        }

        var jobLease = context.TranscodeJobs.Include(j => j.).FirstOrDefault(j => j.Id == transcodeResponse.JobLeaseId);
        if (fileInfo == null || jobLease == null)
        {
            throw new ApplicationException(
                $"File {transcodeResponse.FileInfoId} or lease {transcodeResponse.JobLeaseId} not found");
        }

        if (settings.AutoApplyTranscode)
        {
            if (!File.Exists(jobLease.OutputPath))
            {
                //check attempt and requeue
                return;
            }

            File.Move(jobLease.OutputPath, fileInfo.Path, true);
            fileInfo.ProcessingState = ProcessingState.Succeeded;
            fileInfo.LastModified = DateTimeOffset.UtcNow;
            context.TranscodeJobs.Remove(jobLease);
            context.ProcessingResults.Add(new ProcessingResultEntity
            {
                EncoderSettingsSnapshot = transcodeResponse.EncoderSettingsSnapshot,
                OutputSizeBytes = transcodeResponse.OutputSizeBytes,
                VmafScore = transcodeResponse.VMafScore,
            });
        }
        else
        {
            fileInfo.ProcessingState = ProcessingState.Validating;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "Received heartbeat from {nodeId}")]
    partial void LogReceivedHeartbeatFromNodeId(string nodeId);
}
