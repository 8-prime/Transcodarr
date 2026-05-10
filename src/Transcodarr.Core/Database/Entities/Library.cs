namespace Transcodarr.Core.Database.Entities;

public class Library
{
    public required Guid Id { get; init; }
    public required string FileSystemPath  { get; init; }
    public string? DisplayName { get; init; }
}