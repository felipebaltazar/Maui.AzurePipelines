namespace PipelineApproval.Models;

public class TaskQueue
{
    private CancellationTokenSource _tokenSource;
    private SemaphoreSlim semaphore;
    public TaskQueue()
    {
        semaphore = new SemaphoreSlim(1);
        _tokenSource = new CancellationTokenSource();
    }

    public async Task<T> Enqueue<T>(Func<CancellationToken, Task<T>> taskGenerator, bool cancelPrevious = false)
    {
        if (cancelPrevious)
        {
            _tokenSource.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        await semaphore.WaitAsync();
        try
        {
            var tcs = new TaskCompletionSource<T>();
            _tokenSource.Token.Register(() => tcs.TrySetResult(default(T)), useSynchronizationContext: false);
            return await (await Task.WhenAny(taskGenerator(_tokenSource.Token), tcs.Task));
        }
        finally
        {
            semaphore.Release();
        }
    }
    public async Task Enqueue(Func<CancellationToken, Task> taskGenerator, bool cancelPrevious = false)
    {
        if (cancelPrevious)
        {
            _tokenSource.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        await semaphore.WaitAsync();
        try
        {
            var tcs = new TaskCompletionSource();
            _tokenSource.Token.Register(() => tcs.TrySetResult(), useSynchronizationContext: false);
            await Task.WhenAny(taskGenerator(_tokenSource.Token), tcs.Task);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
