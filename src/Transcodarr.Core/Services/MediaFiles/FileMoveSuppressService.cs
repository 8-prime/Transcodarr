using System.Collections.Concurrent;

namespace Transcodarr.Core.Services.MediaFiles;

public class FileMoveSuppressService
{
    private readonly ConcurrentDictionary<string, byte> _suppressed = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Suppress(string path) => _suppressed.TryAdd(path, 0);

    // Returns true and removes the suppression if the path was suppressed.
    public bool ConsumeIfSuppressed(string path) => _suppressed.TryRemove(path, out _);
}
