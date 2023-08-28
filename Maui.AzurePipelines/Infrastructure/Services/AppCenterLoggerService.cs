using Microsoft.Extensions.Logging;

namespace PipelineApproval.Infrastructure.Services;

public class AppCenterLoggerService : ILogger
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
        //Dummy logger to be used with appcenter
    }

    private class Scope<TState> : IDisposable where TState : notnull
    {
        private readonly TState _state;

        public Scope(TState state)
        {
            _state = state;
        }

        public void Dispose()
        {
        }
    }
}
