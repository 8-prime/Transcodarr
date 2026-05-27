using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class MediaFileEntity
{
    public Guid Id { get; init; }
    public Guid LibraryId { get; init; }
    public LibraryEntity Library { get; init; } = null!;
    public TranscodeStatus Status { get; set; }

    public required string Path { get; set; }
    public DateTimeOffset DiscoveredAt { get; init; }
    public DateTimeOffset FileModifiedAt { get; set; }

    public List<TranscodeJobEntity> Jobs { get; init; } = [];
    public MediaFileMetadataEntity? Metadata { get; set; }
}
