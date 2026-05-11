using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TranscodarrDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TranscodearrDbContext")));


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();