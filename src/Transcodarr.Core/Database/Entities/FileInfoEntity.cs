using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class FileInfoEntity
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public required ProcessingState ProcessingState { get; set; }
    public DateTimeOffset LastModified { get; init; }
    public long Size { get; set; }
    
    // populated after successful probe job
    public string? VideoCodec { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? BitRate { get; set; }
    public bool? IsHdr { get; set; }
    public TimeSpan? Duration { get; set; }
    public long FileSizeBytes { get; set; }
    public string? AudioStreams { get; set; }
}