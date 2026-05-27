using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.DTOs;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Endpoints;

public static class QueueEndpoints
{
    public static IEndpointRouteBuilder MapQueueEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("queue");
        group.MapGet("", GetQueueItems);

        return endpoints;
    }

    private static async Task<Ok<List<QueueItemResponse>>> GetQueueItems([FromServices] TranscodarrDbContext dbContext,
        CancellationToken stoppingToken)
    {
        return TypedResults.Ok(await dbContext
            .TranscodeJobs.Select(j => new QueueItemResponse
            {
                Id = j.Id,
                CreatedAt = j.CreatedAt,
                FileName = j.MediaFile.Path,
                LeaseExpiresAt = j.LeaseExpiresAt,
                LibraryName = j.MediaFile.Library.DisplayName ?? j.MediaFile.Library.FileSystemPath,
                NodeId = j.NodeId,
                Progress = j.Progress,
                Status = j.Status,
            }).ToListAsync(stoppingToken));
    }
}