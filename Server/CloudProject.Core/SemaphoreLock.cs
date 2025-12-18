namespace CloudProject.Core;

public class SemaphoreLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    private SemaphoreLock(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }
    
    public static async Task<SemaphoreLock> AcquireAsync(SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        return new SemaphoreLock(semaphore);
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _semaphore.Release();
    }
}
