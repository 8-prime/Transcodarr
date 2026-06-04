using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.DTOs;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Endpoints;

public static class HistoryEndpoints
{
    public static IEndpointRouteBuilder MapHistoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("history");
        group.MapGet("", GetHistoryItems);

        return endpoints;
    }

    private static async Task<Ok<PagedResponse<HistoryItemResponse>>> GetHistoryItems(
        [FromServices] TranscodarrDbContext dbContext,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken stoppingToken = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await dbContext.TranscodeResults.CountAsync(stoppingToken);

        var items = await dbContext
            .TranscodeResults.OrderByDescending(r => r.CompletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new HistoryItemResponse
            {
                Id = r.Id,
                AudioCodec = r.TranscodeJob.AudioCodec,
                FileName = r.TranscodeJob.MediaFile.Path,
                LibraryName =
                    r.TranscodeJob.MediaFile.Library.DisplayName
                    ?? r.TranscodeJob.MediaFile.Library.FileSystemPath,
                ApprovalState = r.ApprovalState,
                CompletedAt = r.CompletedAt,
                Crf = r.TranscodeJob.ConstantRateFactor,
                DurationSec = r.TranscodeJob.MediaFile.Metadata!.Duration.TotalSeconds,
                EncoderUsed = r.EncoderName,
                InputSizeBytes = r.TranscodeJob.MediaFile.Metadata!.FileSizeBytes,
                OutputSizeBytes = r.FileSizeBytes,
                VideoCodec = r.TranscodeJob.VideoCodec,
                VmafScore = r.VmafScore ?? 0,
            })
            .ToListAsync(stoppingToken);

        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return TypedResults.Ok(
            new PagedResponse<HistoryItemResponse>(items, page, pageSize, total, totalPages)
        );
    }
}
