using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Common.Models;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Services;

public class LibraryStartupScannerService : BackgroundService
{
    private readonly ILogger<LibraryStartupScannerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly JobQueue _jobQueue;

    public LibraryStartupScannerService(ILogger<LibraryStartupScannerService> logger,
        IServiceScopeFactory serviceScopeFactory, JobQueue jobQueue)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _jobQueue = jobQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
        Dictionary<string, LibraryScanInfo> knownFiles =
            await dbContext.FileInfos.AsNoTracking().Select(f => new LibraryScanInfo
            {
                LibraryPath = f.Path,
                LibraryId = f.Id,
                FileSize = f.Size,
                LastModified = f.LastModified,
            }).ToDictionaryAsync(f => f.LibraryPath, stoppingToken);
        var libraries = await dbContext.Libraries.AsNoTracking().ToListAsync(stoppingToken);
        foreach (var library in libraries)
        {
            await ScanLibraryAsync(knownFiles, library.FileSystemPath, stoppingToken);
        }
    }

    private async Task ScanLibraryAsync(Dictionary<string, LibraryScanInfo> knownFiles, string libraryPath,
        CancellationToken stoppingToken)
    {
        foreach (var file in Directory.EnumerateFiles(libraryPath, "*",
                     SearchOption.AllDirectories).Where(f =>
                     FileTypeConstants.IsVideoFileRegex().IsMatch(Path.GetExtension(f))))
        {
            if (!knownFiles.TryGetValue(file, out var libraryScanInfo))
            {
                //TODO actually pass on the file and whatever else may be required
                await _jobQueue.Writer.WriteAsync(new TranscodeJobRequest(), stoppingToken);
                continue;
            }

            var fi = new FileInfo(file);
            if (fi.Length != libraryScanInfo.FileSize ||
                fi.LastWriteTimeUtc != libraryScanInfo.LastModified.UtcDateTime)
            {
                //TODO actually pass on the file and whatever else may be required                
                await _jobQueue.Writer.WriteAsync(new TranscodeJobRequest(), stoppingToken);
            }
        }
    }
}