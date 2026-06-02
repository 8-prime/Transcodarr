using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Transcodarr.Core.Database;

namespace Transcodarr.Core.Services.MediaFiles;

public class LibraryWatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<LibraryWatcherSettings> _settings;
    private readonly ILogger<LibraryWatcherService> _logger;

    public LibraryWatcherService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<LibraryWatcherSettings> settings,
        ILogger<LibraryWatcherService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

            var libraries = await dbContext.Libraries.ToListAsync(stoppingToken);
            foreach (var library in libraries)
            {
                try
                {
                    await libraryService.ScanLibraryAsync(
                        library.FileSystemPath,
                        library.Id,
                        stoppingToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to scan library {LibraryId} at {Path}",
                        library.Id,
                        library.FileSystemPath
                    );
                }
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_settings.CurrentValue.ScanIntervalSeconds),
                stoppingToken
            );
        }
    }
}
