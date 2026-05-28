namespace Transcodarr.Core.Database.Entities;

public class MediaFileMetadataEntity
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }
    public MediaFileEntity MediaFileEntity { get; set; } = null!;

    public required string VideoCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long BitRate { get; set; }
    public bool IsHdr { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }

    public required string AudioStreams { get; set; }
}
