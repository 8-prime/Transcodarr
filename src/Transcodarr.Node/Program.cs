using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<NodeConfiguration>(
    builder.Configuration.GetSection(nameof(NodeConfiguration)));


builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<CapabilitiesService>();
builder.Services.AddSingleton<SlotTracker>();
builder.Services.AddSingleton<NodeInfoManager>();
builder.Services.AddHostedService<WebsocketConnectionService>();
builder.Services.AddHostedService<NodeLifecycleManager>();

var app = builder.Build();

app.Run();