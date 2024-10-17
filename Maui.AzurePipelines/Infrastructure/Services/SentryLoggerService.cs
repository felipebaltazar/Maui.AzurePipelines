using Microsoft.Extensions.Logging;

namespace PipelineApproval.Infrastructure.Services;

public class SentryLoggerService : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return new Scope<TState>(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Debug:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Debug);
                break;
            case LogLevel.Information:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Info);
                break;
            case LogLevel.Warning:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Warning);
                break;
            case LogLevel.Error when exception is not null:
                SentrySdk.CaptureException(exception);
                break;
            case LogLevel.Error when exception is null:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Error);
                break;
            case LogLevel.Critical when exception is not null:
                SentrySdk.CaptureException(exception);
                break;
            case LogLevel.Critical when exception is null:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Critical);
                break;
            case LogLevel.Trace:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Info);
                break;
            default:
                SentrySdk.AddBreadcrumb(formatter(state, exception), level: BreadcrumbLevel.Info);
                break;
        }
    }

    private class Scope<TState> : IDisposable where TState : notnull
    {
        private readonly IDisposable _disposable;
        private readonly TState _state;

        public Scope(TState state)
        {
            _disposable = SentrySdk.PushScope<TState>(state);
            _state = state;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
