using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;
using Transcodarr.Core.Database.Enums;

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
        Dictionary<string, LibraryScanInfo> knownFiles =
            await dbContext.FileInfos.AsNoTracking().Select(f => new LibraryScanInfo
            {
                LibraryPath = f.Path,
                FileInfoId = f.Id,
                LibraryId = f.LibraryId,
                FileSize = f.FileSizeBytes,
                LastModified = f.LastModified,
            }).ToDictionaryAsync(f => f.LibraryPath, stoppingToken);
        var libraries = await dbContext.Libraries.AsNoTracking().ToListAsync(stoppingToken);
        foreach (var library in libraries)
        {
            await ScanLibraryAsync(knownFiles, library.FileSystemPath, dbContext, fileProbeService,
                transcodeEligibilityService, settings, stoppingToken);
        }

        var deletedPaths = knownFiles.Keys.ToList();
        await dbContext.FileInfos.Where(f => deletedPaths.Contains(f.Path)).ExecuteDeleteAsync(stoppingToken);
    }

    private async Task ScanLibraryAsync(Dictionary<string, LibraryScanInfo> knownFiles, string libraryPath,
        TranscodarrDbContext dbContext,
        FileProbeService fileProbeService,
        TranscodeEligibilityService transcodeEligibilityService,
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

            if (fi.Length == libraryScanInfo.FileSize &&
                fi.LastWriteTimeUtc == libraryScanInfo.LastModified.UtcDateTime)
            {
                continue;
            }

            dbContext.TranscodeJobs.Add(new TranscodeJobEntity
            {
                FileInfoId = libraryScanInfo.FileInfoId,
                State = JobState.Pending,
                OutputPath = Path.Join(configurationEntity.TranscodeTempDirectory, Path.GetFileName(fi.FullName),
                    FileTypeConstants.TempFileSuffix),
            });

            var stub = new FileInfoEntity
                { Id = libraryScanInfo.FileInfoId, Path = null!, VideoCodec = null!, AudioStreams = null! };
            dbContext.FileInfos.Attach(stub);
            dbContext.Entry(stub).Property(f => f.ProcessingState).IsModified = true;
            stub.ProcessingState = ProcessingState.Queued;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private static async Task AddNewFile(TranscodarrDbContext dbContext, FileProbeService fileProbeService,
        TranscodeEligibilityService transcodeEligibilityService, string file, FileInfo fi,
        AppConfigurationEntity configurationEntity,
        CancellationToken stoppingToken)
    {
        var probeResult = await fileProbeService.ProbeFileAsync(file, stoppingToken);

        if (probeResult is null)
        {
            //TODO: Add to dlq
            return;
        }

        var newFileInfo = new FileInfoEntity
        {
            Id = Guid.NewGuid(),
            Path = file,
            ProcessingState = ProcessingState.Discovered,
            LastModified = fi.LastWriteTimeUtc,
            AudioStreams = probeResult.AudioStreams,
            VideoCodec = probeResult.VideoCodec,
            BitRate = probeResult.Bitrate,
            Duration = probeResult.Duration,
            FileSizeBytes = fi.Length,
            Height = probeResult.Height,
            Width = probeResult.Width,
            IsHdr = probeResult.IsHdr,
        };
        dbContext.FileInfos.Add(newFileInfo);

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