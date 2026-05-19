using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Shared.DTOs;
using Transcodearr.Shared.DTOs;

namespace Transcodarr.Core.Services;

public class JobQueueManagerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionManager _connections;
    private readonly WebSocketConnectionService _webSocketConnectionService;

    public JobQueueManagerService(IServiceScopeFactory serviceScopeFactory, ConnectionManager connections,
        WebSocketConnectionService webSocketConnectionService)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _connections = connections;
        _webSocketConnectionService = webSocketConnectionService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);


            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var config = await dbContext.AppConfigurations.FirstOrDefaultAsync(cancellationToken: stoppingToken);
            if (config == null)
            {
                throw new ApplicationException("No configuration found");
            }

            var timedOutLeases = await dbContext.TranscodeJobs.Where(j => j.LeaseExpiresAt >= DateTime.UtcNow)
                .ExecuteUpdateAsync(setter =>
                        setter.SetProperty(j => j.LeaseExpiresAt, (DateTime?)null)
                            .SetProperty(j => j.State, JobState.Pending),
                    stoppingToken);


            //check active loads for progress
            //if no progress reported from node and expiry timed out, reset job
            //if progress reported, reset expiry to x minutes in future


            var candiatesCount = _connections.GetTotalFreeSlots();

            var pendingJobs = await dbContext.TranscodeJobs.Include(j => j.FileInfoId)
                .Where(j => j.State == JobState.Pending).Take(candiatesCount)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var pendingJob in pendingJobs)
            {
                var conn = _connections.GetConnections().FirstOrDefault();
                if (conn is null)
                {
                    break;
                }

                var request = new TranscodeRequest(pendingJob.FileInfoEntity.Path, pendingJob.OutputPath,
                    pendingJob.FileInfoId,
                    new TranscodeQualitySettings
                    {
                        ConstantRateFactor = config.ConstantRateFactor,
                        DesiredAudioCodec = config.TranscodeTempAudioCodec,
                        DesiredVideoCodec = config.TranscodeTempVideoCodec,
                        DesiredEncoderPreset = config.TranscodeEncoderPreset,
                    })
                {
                    CorrelationId = Guid.NewGuid(),
                };
                await _webSocketConnectionService.SendFireAndForgetAsync(request, conn, stoppingToken);
            }

            //get predicted available processing slots
            //load predicted count pending jobs from db
            //while job can be enqueued on any node, iterate oder pending jobs and update
            //set job to dispatched and save
            //if http call fails reset dispatched state
        }
    }
}