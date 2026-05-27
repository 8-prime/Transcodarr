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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AppConfigurationEntity>()
            .Property(c => c.TranscodeTempDirectory)
            .HasMaxLength(4096);

        modelBuilder
            .Entity<LibraryEntity>()
            .HasMany<MediaFileEntity>()
            .WithOne(f => f.Library)
            .HasForeignKey(f => f.LibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MediaFileEntity>().Property(f => f.Path).HasMaxLength(4096);

        modelBuilder
            .Entity<MediaFileEntity>()
            .HasOne(f => f.Metadata)
            .WithOne(m => m.MediaFileEntity)
            .HasForeignKey<MediaFileMetadataEntity>(m => m.MediaFileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<MediaFileEntity>()
            .HasMany(f => f.Jobs)
            .WithOne(j => j.MediaFile)
            .HasForeignKey(j => j.MediaFileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<MediaFileMetadataEntity>()
            .Property(m => m.VideoCodec)
            .HasMaxLength(100);

        modelBuilder
            .Entity<MediaFileMetadataEntity>()
            .Property(m => m.AudioStreams)
            .HasMaxLength(1000);

        modelBuilder.Entity<TranscodeJobEntity>().Property(j => j.NodeId).HasMaxLength(256);

        modelBuilder.Entity<TranscodeJobEntity>().Property(j => j.OutputPath).HasMaxLength(4096);

        modelBuilder
            .Entity<TranscodeJobEntity>()
            .HasOne(j => j.TranscodeResult)
            .WithOne(r => r.TranscodeJob)
            .HasForeignKey<TranscodeResultEntity>(r => r.TranscodeJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
