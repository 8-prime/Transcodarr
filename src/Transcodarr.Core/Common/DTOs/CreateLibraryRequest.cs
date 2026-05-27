namespace Transcodarr.Core.Common.DTOs;

public class CreateLibraryRequest
{
    public required string Path { get; init; }
    public string? DisplayName { get; init; }
}
