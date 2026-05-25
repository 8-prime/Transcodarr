namespace Transcodarr.Core.Database.Enums;

public enum TranscodeStatus
{
    Discovered, // found by scanner, not yet probed
    Pending, // needs transcoding, waiting for a job
    InProgress, // job is active
    NotRequired, // probed, already in target format
    Completed, // transcoded successfully
    Failed, // failed, needs manual attention
}
