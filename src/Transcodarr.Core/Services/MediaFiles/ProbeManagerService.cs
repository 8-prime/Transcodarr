using System.Collections.Concurrent;

namespace Transcodarr.Core.Services.MediaFiles;

public class ProbeManagerService
{
    private readonly ConcurrentDictionary<Guid, string> _inFlightProbes = new();

    public bool TryStart(Guid fileId, string nodeId) => _inFlightProbes.TryAdd(fileId, nodeId);

    public void Complete(Guid fileId) => _inFlightProbes.TryRemove(fileId, out _);

    public bool IsInFlight(Guid fileId) => _inFlightProbes.ContainsKey(fileId);

    public IReadOnlyList<Guid> ClearNode(string nodeId)
    {
        var cleared = _inFlightProbes.Where(kv => kv.Value == nodeId).Select(kv => kv.Key).ToList();
        foreach (var id in cleared)
            _inFlightProbes.TryRemove(id, out _);
        return cleared;
    }
}
