using Transcodarr.Core.Endpoints;
using Transcodarr.Core.Services;
using Transcodarr.Core.Services.Configuration;
using Transcodarr.Core.Services.Jobs;
using Transcodarr.Core.Services.MediaFiles;
using MessageHandler = Transcodarr.Core.Services.Connection.MessageHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConfigurationService>();

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddTransient<WebSocketConnectionService>();

builder.Services.AddTransient<JobQueueManagerService>();

builder.Services.AddScoped<FileProbeService>();
builder.Services.AddScoped<LibraryService>();
builder.Services.AddSingleton<TranscodeEligibilityService>();
builder.Services.AddHostedService<LibraryWatcherService>();

var app = builder.Build();

app.UseWebSockets();
app.MapConnection();

app.Run();
