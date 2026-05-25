namespace Transcodarr.Core.Database.Entities;

public class LibraryEntity
{
    public required Guid Id { get; init; }
    public required string FileSystemPath { get; init; }
    public string? DisplayName { get; init; }
}
