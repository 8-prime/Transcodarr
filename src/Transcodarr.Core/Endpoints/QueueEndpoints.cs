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

    private static async Task<Ok<PagedResponse<QueueItemResponse>>> GetQueueItems(
        [FromServices] TranscodarrDbContext dbContext,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken stoppingToken = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var baseQuery = dbContext.MediaFiles.Where(m =>
            m.Status == TranscodeStatus.Discovered
            || m.Status == TranscodeStatus.Pending
            || m.Status == TranscodeStatus.InProgress
            || m.Status == TranscodeStatus.Failed
        );

        var total = await baseQuery.CountAsync(stoppingToken);

        var items = await baseQuery
            .OrderBy(m =>
                m.Status == TranscodeStatus.InProgress ? 0
                : m.Status == TranscodeStatus.Pending ? 1
                : m.Status == TranscodeStatus.Discovered ? 2
                : 3
            )
            .ThenByDescending(m => m.DiscoveredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return TypedResults.Ok(
            new PagedResponse<QueueItemResponse>(items, page, pageSize, total, totalPages)
        );
    }
}
