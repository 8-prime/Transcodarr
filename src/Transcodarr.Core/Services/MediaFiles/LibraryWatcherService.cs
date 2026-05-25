using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Services.MediaFiles;

public class LibraryWatcherService : BackgroundService
{
    private readonly LibraryService _libraryService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<Guid, FileSystemWatcher> _fileSystemWatchers = new();
    private readonly ILogger<LibraryWatcherService> _logger;

    public LibraryWatcherService(LibraryService libraryService, IServiceScopeFactory scopeFactory,
        ILogger<LibraryWatcherService> logger)
    {
        _libraryService = libraryService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var libraries = await dbContext.Libraries.ToDictionaryAsync(l => l.Id, l => l, stoppingToken);
            var newLibraries = libraries.Where(l => !_fileSystemWatchers.ContainsKey(l.Key)).Select(l => l.Value);
            var staleLibraries = _fileSystemWatchers.Where(w => !libraries.ContainsKey(w.Key)).Select(l => l.Key);

            foreach (var newLibrary in newLibraries)
            {
                await SetupFileSystemWatcher(newLibrary.Id, newLibrary.FileSystemPath, stoppingToken);
            }

            foreach (var staleLibraryId in staleLibraries)
            {
                RemoveFileSystemWatcher(staleLibraryId);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
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

    private async Task SetupFileSystemWatcher(Guid libraryId, string fileSystemPath, CancellationToken stoppingToken)
    {
        if (_fileSystemWatchers.ContainsKey(libraryId))
        {
            return;
        }

        var fsWatcher = new FileSystemWatcher(fileSystemPath);
        fsWatcher.Changed += OnFileChanged;
        fsWatcher.Created += OnFileCreated;
        fsWatcher.Deleted += OnFileDeleted;
        fsWatcher.Renamed += OnFileRenamed;
        fsWatcher.EnableRaisingEvents = true;
        await _libraryService.ScanLibraryAsync(fileSystemPath, libraryId, stoppingToken);

        _fileSystemWatchers.Add(libraryId, fsWatcher);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        //check for changes that originate because transcodarr finished transcoding
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
    }

    private void OnFileRenamed(object sender, FileSystemEventArgs e)
    {
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
    }
}