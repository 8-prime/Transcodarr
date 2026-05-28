using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType
                    .ClrType.GetProperties()
                    .Where(p =>
                        p.PropertyType == typeof(DateTimeOffset)
                        || p.PropertyType == typeof(DateTimeOffset?)
                    );
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }
        }

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

        modelBuilder.Entity<TranscodeResultEntity>().Property(r => r.EncoderName).HasMaxLength(100);

        modelBuilder.Entity<MediaFileEntity>().HasIndex(f => f.LibraryId);
        modelBuilder.Entity<MediaFileEntity>().HasIndex(f => f.Status);
        modelBuilder.Entity<MediaFileMetadataEntity>().HasIndex(m => m.MediaFileId).IsUnique();
        modelBuilder.Entity<TranscodeJobEntity>().HasIndex(j => j.MediaFileId);
        modelBuilder.Entity<TranscodeJobEntity>().HasIndex(j => new { j.Status, j.LeaseExpiresAt });
        modelBuilder.Entity<TranscodeResultEntity>().HasIndex(r => r.TranscodeJobId).IsUnique();
    }
}
