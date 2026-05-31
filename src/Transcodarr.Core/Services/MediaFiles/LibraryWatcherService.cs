using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Constants;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Services.MediaFiles;

public class LibraryWatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FileMoveSuppressService _fileMoveSuppressService;
    private readonly Dictionary<Guid, FileSystemWatcher> _fileSystemWatchers = new();
    private readonly ILogger<LibraryWatcherService> _logger;

    private readonly Channel<FileSystemEventArgs> _fileSystemWatcherEvents =
        Channel.CreateBounded<FileSystemEventArgs>(200);

    public LibraryWatcherService(
        IServiceScopeFactory scopeFactory,
        FileMoveSuppressService fileMoveSuppressService,
        ILogger<LibraryWatcherService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _fileMoveSuppressService = fileMoveSuppressService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await HandleFileSystemWatcherEvents(stoppingToken);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            var libraries = await dbContext.Libraries.ToDictionaryAsync(
                l => l.Id,
                l => l,
                stoppingToken
            );
            var newLibraries = libraries
                .Where(l => !_fileSystemWatchers.ContainsKey(l.Key))
                .Select(l => l.Value);
            var staleLibraries = _fileSystemWatchers
                .Where(w => !libraries.ContainsKey(w.Key))
                .Select(l => l.Key);

            foreach (var newLibrary in newLibraries)
            {
                _logger.LogInformation(
                    "New library detected {LibraryId}, setting up watcher for {Path}",
                    newLibrary.Id,
                    newLibrary.FileSystemPath
                );
                await SetupFileSystemWatcher(
                    newLibrary.Id,
                    newLibrary.FileSystemPath,
                    libraryService,
                    stoppingToken
                );
            }

            foreach (var staleLibraryId in staleLibraries)
            {
                _logger.LogInformation(
                    "Removing stale library watcher {LibraryId}",
                    staleLibraryId
                );
                RemoveFileSystemWatcher(staleLibraryId);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task HandleFileSystemWatcherEvents(CancellationToken stoppingToken)
    {
        while (
            !stoppingToken.IsCancellationRequested
            && _fileSystemWatcherEvents.Reader.TryRead(out var fileSystemEventArgs)
        )
        {
            if (
                fileSystemEventArgs is RenamedEventArgs renamed
                && renamed.OldFullPath.EndsWith(
                    FileTypeConstants.TempFileSuffix,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }
            if (
                fileSystemEventArgs.FullPath.EndsWith(
                    FileTypeConstants.TempFileSuffix,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            if (_fileMoveSuppressService.ConsumeIfSuppressed(fileSystemEventArgs.FullPath))
            {
                continue;
            }

            _logger.LogDebug(
                "File system event {ChangeType} for {Path}",
                fileSystemEventArgs.ChangeType,
                fileSystemEventArgs.FullPath
            );

            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            try
            {
                switch (fileSystemEventArgs.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        await HandleFileCreated(stoppingToken, fileSystemEventArgs, scope);
                        break;
                    case WatcherChangeTypes.Deleted:
                        await HandleFileDeleted(stoppingToken, db, fileSystemEventArgs);
                        break;
                    case WatcherChangeTypes.Changed:
                        await HandleFileChanged(stoppingToken, db, fileSystemEventArgs, scope);
                        break;
                    case WatcherChangeTypes.Renamed:
                        await HandleFileRenamed(stoppingToken, fileSystemEventArgs, db, scope);
                        break;
                    case WatcherChangeTypes.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to handle {ChangeType} event for {Path}",
                    fileSystemEventArgs.ChangeType,
                    fileSystemEventArgs.FullPath
                );
            }
        }
    }

    private async Task HandleFileRenamed(
        CancellationToken stoppingToken,
        FileSystemEventArgs fileSystemEventArgs,
        TranscodarrDbContext db,
        AsyncServiceScope scope
    )
    {
        var renamedEvent = (RenamedEventArgs)fileSystemEventArgs;
        var existing = await db.MediaFiles.FirstOrDefaultAsync(
            f => f.Path == renamedEvent.OldFullPath,
            stoppingToken
        );
        if (existing is not null)
        {
            existing.Path = renamedEvent.FullPath;
            return;
        }

        if (GetLibraryIdForPath(renamedEvent.FullPath) is not { } libraryId)
        {
            return;
        }

        var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
        libraryService.AddNewFile(fileSystemEventArgs.FullPath, libraryId);
    }

    private static async Task HandleFileChanged(
        CancellationToken stoppingToken,
        TranscodarrDbContext db,
        FileSystemEventArgs fileSystemEventArgs,
        AsyncServiceScope scope
    )
    {
        var changed = await db.MediaFiles.FirstOrDefaultAsync(
            f => f.Path == fileSystemEventArgs.FullPath,
            stoppingToken
        );
        if (changed is not null)
        {
            var fileProbeService = scope.ServiceProvider.GetRequiredService<FileProbeService>();
            changed.FileModifiedAt = new FileInfo(fileSystemEventArgs.FullPath).LastWriteTimeUtc;
            changed.Status = TranscodeStatus.Discovered;
            await fileProbeService.ProbeFileAsync(changed.Path, changed.Id, stoppingToken);
        }
    }

    private static async Task HandleFileDeleted(
        CancellationToken stoppingToken,
        TranscodarrDbContext db,
        FileSystemEventArgs fileSystemEventArgs
    )
    {
        var deleted = await db.MediaFiles.FirstOrDefaultAsync(
            f => f.Path == fileSystemEventArgs.FullPath,
            stoppingToken
        );
        if (deleted is not null)
        {
            db.MediaFiles.Remove(deleted);
        }
    }

    private async Task HandleFileCreated(
        CancellationToken stoppingToken,
        FileSystemEventArgs fileSystemEventArgs,
        AsyncServiceScope scope
    )
    {
        if (
            !FileTypeConstants
                .IsVideoFileRegex()
                .IsMatch(Path.GetExtension(fileSystemEventArgs.FullPath))
        )
        {
            return;
        }

        var libraryId = GetLibraryIdForPath(fileSystemEventArgs.FullPath);
        if (libraryId is null)
        {
            return;
        }

        var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
        libraryService.AddNewFile(fileSystemEventArgs.FullPath, libraryId.Value);
    }

    private Guid? GetLibraryIdForPath(string filePath)
    {
        foreach (var (id, watcher) in _fileSystemWatchers)
        {
            if (filePath.StartsWith(watcher.Path, StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }
        }

        return null;
    }

    private void RemoveFileSystemWatcher(Guid libraryId)
    {
        if (!_fileSystemWatchers.TryGetValue(libraryId, out var fsWatcher))
        {
            return;
        }

        fsWatcher.Dispose();
        _fileSystemWatchers.Remove(libraryId);
    }

    private async Task SetupFileSystemWatcher(
        Guid libraryId,
        string fileSystemPath,
        LibraryService libraryService,
        CancellationToken stoppingToken
    )
    {
        if (_fileSystemWatchers.ContainsKey(libraryId))
        {
            return;
        }

        var fsWatcher = new FileSystemWatcher(fileSystemPath);
        fsWatcher.IncludeSubdirectories = true;
        fsWatcher.Changed += OnFileSystemWatcherEvent;
        fsWatcher.Created += OnFileSystemWatcherEvent;
        fsWatcher.Deleted += OnFileSystemWatcherEvent;
        fsWatcher.Renamed += OnFileSystemWatcherEvent;
        fsWatcher.EnableRaisingEvents = true;
        await libraryService.ScanLibraryAsync(fileSystemPath, libraryId, stoppingToken);

        _fileSystemWatchers.Add(libraryId, fsWatcher);
    }

    private void OnFileSystemWatcherEvent(object sender, FileSystemEventArgs e)
    {
        _fileSystemWatcherEvents.Writer.TryWrite(e);
    }
}
