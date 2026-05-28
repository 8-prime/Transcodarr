using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;
using Transcodarr.Core.Endpoints;
using Transcodarr.Core.Services;
using Transcodarr.Core.Services.Configuration;
using Transcodarr.Core.Services.Connection;
using Transcodarr.Core.Services.Jobs;
using Transcodarr.Core.Services.MediaFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

builder.Services.AddDbContextFactory<TranscodarrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TranscodarrDb"))
);

builder.Services.AddSingleton<ConfigurationService>();

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddTransient<WebSocketConnectionService>();

builder.Services.AddHostedService<JobQueueManagerService>();

builder.Services.AddScoped<FileProbeService>();
builder.Services.AddScoped<LibraryService>();
builder.Services.AddSingleton<TranscodeEligibilityService>();
builder.Services.AddHostedService<LibraryWatcherService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TranscodarrDbContext>();
    await dbContext.Database.MigrateAsync();

    var configuration = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
    try
    {
        await configuration.InitializeAsync(CancellationToken.None);
    }
    catch { }
}

app.UseWebSockets();
app.MapConnections();
app.MapHistoryEndpoints();
app.MapLibraryEndpoints();
app.MapQueueEndpoints();
app.MapSettingsEndpoints();

app.Run();
