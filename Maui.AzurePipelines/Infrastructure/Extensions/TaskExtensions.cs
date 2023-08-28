namespace PipelineApproval.Infrastructure.Extensions;

/// <summary>
/// Based on <see href="https://github.com/brminnick/AsyncAwaitBestPractices"/>
/// </summary>
public static class TaskExtensions
{
    private static Action<Exception> _onException;
    private static bool _shouldAlwaysRethrowException;

    public static void SafeFireAndForget(this ValueTask task, bool continueOnCapturedContext = false, Action<Exception> onException = null) => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget(this Task task, bool continueOnCapturedContext = false, Action<Exception> onException = null) => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void SafeFireAndForget<TException>(this Task task, bool continueOnCapturedContext = false, Action<TException> onException = null) where TException : Exception => HandleSafeFireAndForget(task, continueOnCapturedContext, onException);

    public static void Initialize(bool shouldAlwaysRethrowException = false) => _shouldAlwaysRethrowException = shouldAlwaysRethrowException;

    public static void SetDefaultExceptionHandling(Action<Exception> onException)
    {
        if (onException is null)
            throw new ArgumentNullException(nameof(onException), $"{onException} cannot be null");

        _onException = onException;
    }

    public static void RemoveDefaultExceptionHandling() => _onException = null;

    private static async void HandleSafeFireAndForget<TException>(Task task, bool continueOnCapturedContext, Action<TException> onException) where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (_onException != null || onException != null)
        {
            _onException?.Invoke(ex);
            onException?.Invoke(ex);

            if (_shouldAlwaysRethrowException)
                throw;
        }
    }

    private static async void HandleSafeFireAndForget<TException>(ValueTask task, bool continueOnCapturedContext, Action<TException> onException) where TException : Exception
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (TException ex) when (_onException != null || onException != null)
        {
            _onException?.Invoke(ex);
            onException?.Invoke(ex);

            if (_shouldAlwaysRethrowException)
                throw;
        }
    }
}

