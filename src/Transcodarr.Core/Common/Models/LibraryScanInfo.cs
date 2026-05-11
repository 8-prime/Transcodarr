namespace Transcodarr.Core.Common.Models;

public struct LibraryScanInfo
{
    public Guid LibraryId { get; set; }
    public string LibraryPath { get; set; }
    public long FileSize { get; set; }
    public DateTimeOffset  LastModified { get; set; }
}