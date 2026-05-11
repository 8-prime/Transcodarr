using Transcodarr.Core.Common.Enums;

namespace Transcodarr.Core.Common.Events;

public abstract record LibraryChangeEvent(Guid LibraryId);
public record LibraryAdded(Guid LibraryId, string Path) : LibraryChangeEvent(LibraryId);
public record LibraryRemoved(Guid LibraryId) : LibraryChangeEvent(LibraryId);
public record LibraryUpdated(Guid LibraryId, string Path) : LibraryChangeEvent(LibraryId);