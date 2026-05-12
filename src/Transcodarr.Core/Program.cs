using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;
using Transcodarr.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TranscodarrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TranscodarrDbContext")));

builder.Services.AddHostedService<LibraryStartupScannerService>();
builder.Services.AddHostedService<LibraryWatcherService>();
builder.Services.AddHostedService<JobQueueManagerService>();
builder.Services.AddScoped<FileProbeService>();
builder.Services.AddScoped<TranscodeEligibilityService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();