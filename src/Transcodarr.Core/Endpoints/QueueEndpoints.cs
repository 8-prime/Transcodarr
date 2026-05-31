using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.DTOs;
using Transcodarr.Core.Common.DTOs.Enums;
using Transcodarr.Core.Common.Extensions;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Endpoints;

public static class QueueEndpoints
{
    public static IEndpointRouteBuilder MapQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("queue");
        group.MapGet("", GetQueueItems);

        return endpoints;
    }

    private static async Task<Ok<IEnumerable<QueueItemResponse>>> GetQueueItems(
        [FromServices] TranscodarrDbContext dbContext,
        CancellationToken stoppingToken
    )
    {
        var items = await dbContext
            .MediaFiles.Where(m =>
                m.Status == TranscodeStatus.Discovered
                || m.Status == TranscodeStatus.Pending
                || m.Status == TranscodeStatus.InProgress
                || m.Status == TranscodeStatus.Failed
            )
            .Select(m => new QueueItemResponse
            {
                Id = m.Id,
                FileName = m.Path,
                LibraryName = m.Library.DisplayName ?? m.Library.FileSystemPath,
                State = QueueItemStatus.FromMediaFileStatus(m.Status),
                AttemptNumber = m.Jobs.Count,
                CreatedAt = m.DiscoveredAt,
                TargetCodec =
                    m.Jobs.Where(j => j.Status == TranscodeJobStatus.Processing)
                        .OrderByDescending(j => j.CreatedAt)
                        .Select(j => j.VideoCodec.ToString())
                        .FirstOrDefault()
                    ?? string.Empty,
                NodeId = m
                    .Jobs.Where(j => j.Status == TranscodeJobStatus.Processing)
                    .OrderByDescending(j => j.CreatedAt)
                    .Select(j => j.NodeId)
                    .FirstOrDefault(),
                ProgressPct = m
                    .Jobs.Where(j => j.Status == TranscodeJobStatus.Processing)
                    .OrderByDescending(j => j.CreatedAt)
                    .Select(j => (double?)j.Progress)
                    .FirstOrDefault(),
            })
            .ToArrayAsync(stoppingToken);

        return TypedResults.Ok<IEnumerable<QueueItemResponse>>(items);
    }
}
