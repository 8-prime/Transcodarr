namespace Transcodarr.Core.Common.DTOs;

public class FilesystemBrowseResponse
{
    public required List<FilesystemEntryResponse> Directories { get; init; }
}

public class FilesystemEntryResponse
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
