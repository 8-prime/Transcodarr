using Transcodarr.Node.Common.Models;
using Transcodarr.Node.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NodeConfiguration>(
    builder.Configuration.GetSection(nameof(NodeConfiguration)));
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddHostedService<WebsocketConnectionService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();