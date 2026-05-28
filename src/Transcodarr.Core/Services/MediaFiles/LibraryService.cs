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
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(
        TranscodarrDbContext dbContext,
        FileProbeService fileProbeService,
        ILogger<LibraryService> logger
    )
    {
        _dbContext = dbContext;
        _fileProbeService = fileProbeService;
        _logger = logger;
    }

    public async Task ScanLibraryAsync(
        string libraryPath,
        Guid libraryId,
        CancellationToken stoppingToken
    )
    {
        _logger.LogInformation("Scanning library {LibraryPath}", libraryPath);

        var knownFiles = await _dbContext
            .MediaFiles.AsNoTracking()
            .Include(f => f.Library)
            .ToDictionaryAsync(f => f.Path, stoppingToken);

        var newCount = 0;
        var changedCount = 0;

        foreach (
            var file in Directory
                .EnumerateFiles(libraryPath, "*", SearchOption.AllDirectories)
                .Where(f => FileTypeConstants.IsVideoFileRegex().IsMatch(Path.GetExtension(f)))
        )
        {
            var fi = new FileInfo(file);
            if (!knownFiles.Remove(file, out var libraryScanInfo))
            {
                _logger.LogInformation("Discovered new file {FilePath}", file);
                await AddNewFile(file, libraryId, stoppingToken);
                newCount++;
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

            _logger.LogInformation("File changed, re-probing {FilePath}", file);
            mediaFile.Metadata = null;
            mediaFile.Status = TranscodeStatus.Discovered;
            await _fileProbeService.ProbeFileAsync(file, mediaFile.Id, stoppingToken);
            changedCount++;
        }

        _logger.LogInformation(
            "Library scan complete for {LibraryPath}: {NewCount} new, {ChangedCount} changed",
            libraryPath,
            newCount,
            changedCount
        );
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
