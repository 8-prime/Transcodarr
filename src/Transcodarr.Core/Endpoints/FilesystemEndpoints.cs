using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Transcodarr.Core.Common.DTOs;

namespace Transcodarr.Core.Endpoints;

public static class FilesystemEndpoints
{
    public static IEndpointRouteBuilder MapFilesystemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("filesystem");
        group.MapGet("browse", Browse);
        return endpoints;
    }

    private static Results<Ok<FilesystemBrowseResponse>, BadRequest<string>> Browse(
        [FromQuery] string? path
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (OperatingSystem.IsWindows())
                {
                    var drives = DriveInfo
                        .GetDrives()
                        .Select(d => new FilesystemEntryResponse
                        {
                            Name = d.Name,
                            Path = d.Name,
                            LastModified = DateTimeOffset.MinValue,
                        })
                        .ToList();
                    return TypedResults.Ok(new FilesystemBrowseResponse { Directories = drives });
                }

                path = "/";
            }

            if (path.Contains(".."))
            {
                return TypedResults.BadRequest("Invalid path");
            }

            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                return TypedResults.BadRequest("Path does not exist");
            }

            var sep = Path.DirectorySeparatorChar;

            var directories = dirInfo
                .EnumerateDirectories()
                .Where(e => !e.Name.StartsWith('.'))
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .Select(d => new FilesystemEntryResponse
                {
                    Name = d.Name,
                    Path = d.FullName.TrimEnd(sep) + sep,
                    LastModified = new DateTimeOffset(d.LastWriteTimeUtc, TimeSpan.Zero),
                })
                .ToList();
            return TypedResults.Ok(new FilesystemBrowseResponse { Directories = directories });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.BadRequest("Access denied");
        }
        catch (DirectoryNotFoundException)
        {
            return TypedResults.BadRequest("Path does not exist");
        }
        catch
        {
            return TypedResults.BadRequest("Failed to browse path");
        }
    }
}
