namespace Transcodarr.Core.Common.DTOs;

public class LibraryResponse
{
    public required Guid Id { get; init; }
    public required string Path { get; init; }
    public string? DisplayName { get; init; }
    public required int FileCount { get; init; }
    public required bool Watching { get; init; }
}
