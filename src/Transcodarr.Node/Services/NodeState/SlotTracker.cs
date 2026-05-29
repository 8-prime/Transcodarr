using Transcodarr.Shared;

namespace Transcodarr.Node.Services.NodeState;

public class SlotTracker
{
    private readonly Dictionary<string, SemaphoreSlim> _groupSemaphores = [];
    private readonly Dictionary<string, string> _encoderToGroup = [];

    public void Initialize(
        IEnumerable<EncoderCapability> capabilities,
        IReadOnlyDictionary<string, int> groupCapacities
    )
    {
        foreach (var capability in capabilities)
            _encoderToGroup[capability.EncoderName] = capability.SlotGroup;

        foreach (var (group, capacity) in groupCapacities)
            _groupSemaphores[group] = new SemaphoreSlim(capacity, capacity);
    }

    public bool TryAcquire(string encoderName)
    {
        return _encoderToGroup.TryGetValue(encoderName, out var group)
            && _groupSemaphores.TryGetValue(group, out var semaphore)
            && semaphore.Wait(0);
    }

    public void Release(string encoderName)
    {
        if (
            _encoderToGroup.TryGetValue(encoderName, out var group)
            && _groupSemaphores.TryGetValue(group, out var semaphore)
        )
        {
            semaphore.Release();
        }
    }

    public int AvailableSlots => _groupSemaphores.Values.Sum(s => s.CurrentCount);

    public IEnumerable<string> EncodersWithCapacity =>
        _encoderToGroup
            .Where(kvp => _groupSemaphores.TryGetValue(kvp.Value, out var s) && s.CurrentCount > 0)
            .Select(kvp => kvp.Key);
}
