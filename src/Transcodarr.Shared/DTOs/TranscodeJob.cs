namespace Transcodarr.Shared;

public class TranscodeJob
{
    public required string FilePath { get; init; }
    public required Guid JobLease { get; init; }
}
