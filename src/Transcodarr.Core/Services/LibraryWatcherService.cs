using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Common.Enums;
using Transcodarr.Core.Common.Events;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Services;

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
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var libraries = await dbContext.Libraries.ToListAsync(stoppingToken);

            foreach (var library in libraries)
            {
                SetupFileSystemWatcher(library.Id, library.FileSystemPath);
            }
        }

        await foreach (var libraryChange in _libraryService.Reader.ReadAllAsync(stoppingToken))
        {
            switch (libraryChange)
            {
                case LibraryAdded added:
                    SetupFileSystemWatcher(added.LibraryId, added.Path);
                    break;
                case LibraryUpdated updated:
                    RemoveFileSystemWatcher(updated.LibraryId);
                    SetupFileSystemWatcher(updated.LibraryId, updated.Path);
                    break;
                case LibraryRemoved removed:
                    RemoveFileSystemWatcher(removed.LibraryId);
                    break;
                default:
                    throw new UnreachableException();
            }
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

    private void SetupFileSystemWatcher(Guid libraryId, string fileSystemPath)
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