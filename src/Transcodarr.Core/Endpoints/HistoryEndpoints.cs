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

    private static async Task<Ok<List<HistoryItemResponse>>> GetHistoryItems(
        [FromServices] TranscodarrDbContext dbContext, CancellationToken stoppingToken)
    {
        return TypedResults.Ok(await dbContext.TranscodeResults
            .Select(r => new HistoryItemResponse
            {
                Id = r.Id,
                AudioCodec = r.TranscodeJob.AudioCodec,
                FileName = r.TranscodeJob.MediaFile.Path,
                LibraryName = r.TranscodeJob.MediaFile.Library.DisplayName ??
                              r.TranscodeJob.MediaFile.Library.FileSystemPath,
                ApprovalState = r.ApprovalState,
                CompletedAt = r.CompletedAt,
                Crf = r.TranscodeJob.ConstantRateFactor,
                Duration = r.TranscodeJob.MediaFile.Metadata!.Duration,
                EncoderUsed = r.TranscodeJob.NodeId, //TODO node must send info about utilized transcoder
                InputSizeBytes = r.TranscodeJob.MediaFile.Metadata!.FileSizeBytes,
                OutputSizeBytes = r.FileSizeBytes,
                VideoCodec = r.TranscodeJob.VideoCodec,
                VmafScore = r.VmafScore
            }).ToListAsync(stoppingToken));
    }
}