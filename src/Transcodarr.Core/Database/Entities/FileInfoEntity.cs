using System.ComponentModel.DataAnnotations;
using Transcodarr.Core.Database.Enums;

namespace Transcodarr.Core.Database.Entities;

public class FileInfoEntity
{
    public Guid Id { get; init; }
    public Guid LibraryId { get; init; }
    [MaxLength(4096)] public required string Path { get; init; }
    public ProcessingState ProcessingState { get; set; }
    public DateTimeOffset LastModified { get; set; }

    // populated after successful probe job
    [MaxLength(100)] public required string VideoCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long BitRate { get; set; }
    public bool IsHdr { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }
    [MaxLength(1000)] public required string AudioStreams { get; set; }
}