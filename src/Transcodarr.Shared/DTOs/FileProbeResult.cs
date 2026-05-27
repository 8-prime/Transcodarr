namespace Transcodarr.Shared.DTOs;

public class FileProbeResult
{
    public required string VideoCodec { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Bitrate { get; set; }
    public bool IsHdr { get; set; }
    public TimeSpan Duration { get; set; }
    public required string AudioStreams { get; set; }
}
