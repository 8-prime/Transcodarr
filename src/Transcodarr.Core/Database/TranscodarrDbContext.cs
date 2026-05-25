using Microsoft.EntityFrameworkCore;
using Transcodarr.Core.Database.Entities;

namespace Transcodarr.Core.Database;

public class TranscodarrDbContext : DbContext
{
    public DbSet<AppConfigurationEntity> AppConfigurations { get; set; }
    public DbSet<LibraryEntity> Libraries { get; set; }
    public DbSet<MediaFileEntity> MediaFiles { get; set; }
    public DbSet<TranscodeJobEntity> TranscodeJobs { get; set; }
    public DbSet<TranscodeResultEntity> TranscodeResults { get; set; }

    public TranscodarrDbContext(DbContextOptions<TranscodarrDbContext> options)
        : base(options) { }
}
