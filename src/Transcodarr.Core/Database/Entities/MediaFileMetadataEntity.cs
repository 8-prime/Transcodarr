namespace Transcodarr.Core.Database.Entities;

public class MediaFileMetadataEntity
{
    public Guid Id { get; init; }
    public Guid MediaFileId { get; init; }
    public MediaFileEntity MediaFileEntity { get; init; } = null!;

    public required string VideoCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long BitRate { get; set; }
    public bool IsHdr { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }

    public required string AudioStreams { get; set; }
}
