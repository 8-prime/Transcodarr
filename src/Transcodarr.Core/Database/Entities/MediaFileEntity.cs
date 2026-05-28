using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class MediaFileEntity
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public LibraryEntity Library { get; set; } = null!;
    public TranscodeStatus Status { get; set; }

    public required string Path { get; set; }
    public DateTimeOffset DiscoveredAt { get; set; }
    public DateTimeOffset FileModifiedAt { get; set; }

    public List<TranscodeJobEntity> Jobs { get; set; } = [];
    public MediaFileMetadataEntity? Metadata { get; set; }
}
