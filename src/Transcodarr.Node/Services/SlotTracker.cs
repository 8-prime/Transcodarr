using Transcodearr.Shared;

namespace Transcodarr.Node.Services;

public class SlotTracker
{
    private readonly Dictionary<string, SemaphoreSlim> _semaphores = [];

    public void Initialize(IEnumerable<EncoderCapability> capabilities)
    {
        foreach (var capability in capabilities)
        {
            _semaphores.TryAdd(capability.EncoderName, new SemaphoreSlim(capability.Slots, capability.Slots));
        }
    }

    public bool TryAcquire(string encoderName)
    {
        return _semaphores.TryGetValue(encoderName, out var semaphore) && semaphore.Wait(0);
    }

    public void Release(string encoderName)
    {
        if (!_semaphores.TryGetValue(encoderName, out var semaphore))
        {
            return;
        }

        semaphore.Release();
    }

    public int AvailableSlots => _semaphores.Select(x => x.Value.CurrentCount).Sum();
    
    public IEnumerable<string> EncodersWithCapacity =>
        _semaphores.Where(kvp => kvp.Value.CurrentCount > 0).Select(kvp => kvp.Key);
}