using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;
using Transcodarr.Core.Services.MediaFiles;

namespace Transcodarr.Core.Services;

public class LibraryStartupScannerService(IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
        var settings = await dbContext.AppConfigurations.FirstOrDefaultAsync(cancellationToken: stoppingToken);
        if (settings is null)
        {
            throw new ApplicationException("App configuration not found");
        }

        var fileProbeService = scope.ServiceProvider.GetRequiredService<FileProbeService>();
        var transcodeEligibilityService = scope.ServiceProvider.GetRequiredService<TranscodeEligibilityService>();
        var knownFiles =
            await dbContext.MediaFiles.AsNoTracking().Include(f => f.Library)
                .ToDictionaryAsync(f => f.Library.FileSystemPath, stoppingToken);
        var libraries = await dbContext.Libraries.AsNoTracking().ToListAsync(stoppingToken);
        foreach (var library in libraries)
        {
            await ScanLibraryAsync(knownFiles, library.FileSystemPath, dbContext, fileProbeService,
                transcodeEligibilityService, settings, stoppingToken);
        }

        var deletedPaths = knownFiles.Keys.ToList();
        await dbContext.MediaFiles.Where(f => deletedPaths.Contains(f.Path)).ExecuteDeleteAsync(stoppingToken);
    }

    private async Task ScanLibraryAsync(Dictionary<string, MediaFileEntity> knownFiles, string libraryPath,
        Guid libraryId,
        TranscodarrDbContext dbContext,
        AppConfigurationEntity configurationEntity,
        CancellationToken stoppingToken)
    {
        foreach (var file in Directory.EnumerateFiles(libraryPath, "*",
                     SearchOption.AllDirectories).Where(f =>
                     FileTypeConstants.IsVideoFileRegex().IsMatch(Path.GetExtension(f))))
        {
            var fi = new FileInfo(file);
            if (!knownFiles.Remove(file, out var libraryScanInfo))
            {
                await AddNewFile(dbContext, fileProbeService, transcodeEligibilityService, file, fi,
                    configurationEntity, stoppingToken);
                continue;
            }

            if (fi.LastWriteTimeUtc == libraryScanInfo.FileModifiedAt)
            {
                continue;
            }

            var mediaFile =
                await dbContext.MediaFiles.FirstOrDefaultAsync(f => f.Path == file, stoppingToken);
            if (mediaFile is null)
            {
                continue;
            }

            mediaFile.Metadata = null;
            mediaFile.Status = TranscodeStatus.Discovered;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task AddNewFile(TranscodarrDbContext dbContext, FileProbeService fileProbeService,
        TranscodeEligibilityService transcodeEligibilityService, string file, FileInfo fi,
        AppConfigurationEntity configurationEntity,
        CancellationToken stoppingToken)
    {
        var newFileInfo = new MediaFileEntity
        {
            Id = Guid.NewGuid(),
            Path = file,
            Status = TranscodeStatus.Discovered,
            FileModifiedAt = fi.LastWriteTimeUtc,
            DiscoveredAt = DateTimeOffset.UtcNow,
            LibraryId = 
        };
        dbContext.MediaFiles.Add(newFileInfo);

        if (!transcodeEligibilityService.IsEligibleForTranscode(probeResult))
        {
            newFileInfo.ProcessingState = ProcessingState.Ignored;
            return;
        }

        dbContext.TranscodeJobs.Add(new TranscodeJobEntity
        {
            FileInfoId = newFileInfo.Id,
            State = JobState.Pending,
            OutputPath = Path.Join(configurationEntity.TranscodeTempDirectory, Path.GetFileName(fi.FullName),
                FileTypeConstants.TempFileSuffix)
        });

        newFileInfo.ProcessingState = ProcessingState.Queued;
    }
}