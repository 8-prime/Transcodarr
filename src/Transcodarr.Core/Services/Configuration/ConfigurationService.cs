using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;
using Transcodarr.Core.Database.Entities;

namespace Transcodarr.Core.Services.Configuration;

public class ConfigurationService
{
    private AppConfigurationEntity? _config;
    private readonly IDbContextFactory<TranscodarrDbContext> _dbFactory;

    public ConfigurationService(IDbContextFactory<TranscodarrDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public AppConfigurationEntity Current =>
        _config ?? throw new InvalidOperationException("Configuration not loaded");

    public bool Initialized => _config is not null;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        _config =
            await db.AppConfigurations.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No configuration found in database");
    }

    public async Task UpdateAsync(
        Action<AppConfigurationEntity> apply,
        CancellationToken ct = default
    )
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (!Initialized)
        {
            var newSettings = new AppConfigurationEntity
            {
                Id = Guid.NewGuid(),
                TranscodeTempDirectory = "",
            };
            apply(newSettings);
            _config = newSettings;
            db.AppConfigurations.Add(newSettings);
        }
        else
        {
            apply(_config!);
            db.AppConfigurations.Update(_config!);
        }
        await db.SaveChangesAsync(ct);
    }
}
