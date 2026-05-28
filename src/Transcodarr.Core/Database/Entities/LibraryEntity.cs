namespace Transcodarr.Core.Database.Entities;

public class LibraryEntity
{
    public required Guid Id { get; set; }
    public required string FileSystemPath { get; set; }
    public string? DisplayName { get; set; }
}
