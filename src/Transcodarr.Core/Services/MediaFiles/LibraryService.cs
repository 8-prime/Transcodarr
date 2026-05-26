using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Services.MediaFiles;

public class LibraryService
{
    private readonly TranscodarrDbContext _dbContext;
    private readonly FileProbeService _fileProbeService;

    public LibraryService(TranscodarrDbContext dbContext, FileProbeService fileProbeService)
    {
        _dbContext = dbContext;
        _fileProbeService = fileProbeService;
    }

    public async Task ScanLibraryAsync(
        string libraryPath,
        Guid libraryId,
        CancellationToken stoppingToken
    )
    {
        var knownFiles = await _dbContext
            .MediaFiles.AsNoTracking()
            .Include(f => f.Library)
            .ToDictionaryAsync(f => f.Path, stoppingToken);

        foreach (
            var file in Directory
                .EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories)
                .Where(f => FileTypeConstants.IsVideoFileRegex().IsMatch(Path.GetExtension(f)))
        )
        {
            var fi = new FileInfo(file);
            if (!knownFiles.Remove(file, out var libraryScanInfo))
            {
                await AddNewFile(file, libraryId, stoppingToken);
                continue;
            }

            if (fi.LastWriteTimeUtc == libraryScanInfo.FileModifiedAt)
            {
                continue;
            }

            var mediaFile = await _dbContext.MediaFiles.FirstOrDefaultAsync(
                f => f.Path == file,
                stoppingToken
            );
            if (mediaFile is null)
            {
                continue;
            }

            mediaFile.Metadata = null;
            mediaFile.Status = TranscodeStatus.Discovered;
            await _fileProbeService.ProbeFileAsync(file, mediaFile.Id, stoppingToken);
        }
    }

    public async Task AddNewFile(string file, Guid libraryId, CancellationToken stoppingToken)
    {
        var fi = new FileInfo(file);
        var newFileInfo = new MediaFileEntity
        {
            Id = Guid.NewGuid(),
            Path = file,
            Status = TranscodeStatus.Discovered,
            FileModifiedAt = fi.LastWriteTimeUtc,
            DiscoveredAt = DateTimeOffset.UtcNow,
            LibraryId = libraryId,
        };
        _dbContext.MediaFiles.Add(newFileInfo);

        await _fileProbeService.ProbeFileAsync(file, newFileInfo.Id, stoppingToken);
    }
}
