using Serilog;
using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services.Connection;
using Transcodarr.Node.Services.NodeState;
using Transcodarr.Node.Services.Transcoding;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.Configure<NodeConfiguration>(
    builder.Configuration.GetSection(nameof(NodeConfiguration))
);

//Connection
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddHostedService<WebsocketConnectionService>();
builder.Services.AddSingleton<MessagesQueue>();

//Node state
builder.Services.AddSingleton<CapabilitiesService>();
builder.Services.AddSingleton<SlotTracker>();
builder.Services.AddSingleton<NodeInfoManager>();
builder.Services.AddHostedService<NodeLifecycleManager>();

//Transcoding
builder.Services.AddScoped<FileProbeService>();
builder.Services.AddSingleton<TranscodeManager>();
builder.Services.AddScoped<TranscodeService>();
builder.Services.AddSingleton<TranscodesQueue>();

var app = builder.Build();

await app.RunAsync();
