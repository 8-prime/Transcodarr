namespace Transcodarr.Node.Services;

public class SlotTracker
{
    private SemaphoreSlim? _semaphore;

    public void Initialize(int totalSlots)
    {
        _semaphore = new SemaphoreSlim(totalSlots, totalSlots);
    }

    public bool TryAcquire()
    {
        return _semaphore is not null && _semaphore.Wait(0);
    }

    public void Release()
    {
        _semaphore?.Release();
    }

    public int AvailableSlots => _semaphore?.CurrentCount ?? 0;
}