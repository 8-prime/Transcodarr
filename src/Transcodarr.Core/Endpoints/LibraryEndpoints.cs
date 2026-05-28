using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.DTOs;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Services.Configuration;

namespace Transcodarr.Core.Endpoints;

public static class LibraryEndpoints
{
    public static IEndpointRouteBuilder MapLibraryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("libraries");
        group.MapGet("", GetLibraries);
        group.MapPost("", AddLibrary);
        group.MapDelete("{id:guid}", DeleteLibrary);

        return endpoints;
    }

    private static async Task<Ok<List<LibraryResponse>>> GetLibraries(
        [FromServices] TranscodarrDbContext dbContext,
        CancellationToken stoppingToken
    )
    {
        var libraries = await dbContext
            .Libraries.Select(l => new LibraryResponse
            {
                Id = l.Id,
                Path = l.FileSystemPath,
                Name = l.DisplayName ?? l.FileSystemPath,
                FileCount = dbContext.MediaFiles.Count(f => f.LibraryId == l.Id),
                Watching = true,
            })
            .ToListAsync(stoppingToken);
        return TypedResults.Ok(libraries);
    }

    private static async Task<Results<Ok, BadRequest<string>>> AddLibrary(
        [FromBody] CreateLibraryRequest request,
        [FromServices] ConfigurationService configurationService,
        [FromServices] TranscodarrDbContext dbContext,
        CancellationToken stoppingToken
    )
    {
        if (!configurationService.Initialized)
        {
            return TypedResults.BadRequest(
                "Settings must be initialized before libraries can be added"
            );
        }

        dbContext.Libraries.Add(
            new LibraryEntity
            {
                Id = Guid.NewGuid(),
                FileSystemPath = request.Path,
                DisplayName = request.DisplayName,
            }
        );

        return await dbContext.SaveChangesAsync(stoppingToken) == 1
            ? TypedResults.Ok()
            : TypedResults.BadRequest("Failed to add library");
    }

    private static async Task<Results<Ok, NotFound>> DeleteLibrary(
        Guid id,
        [FromServices] TranscodarrDbContext dbContext,
        CancellationToken stoppingToken
    )
    {
        return
            await dbContext.Libraries.Where(l => l.Id == id).ExecuteDeleteAsync(stoppingToken) == 0
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
