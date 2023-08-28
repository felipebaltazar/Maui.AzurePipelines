using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Extensions;
using System.Reflection;
using System.Windows.Input;

namespace PipelineApproval.Infrastructure.Commands;

public class AsyncCommand<T> : IAsyncCommand<T>
{
    readonly Func<T, Task> _execute;
    readonly Func<object, bool> _canExecute;
    readonly Action<Exception> _onException;
    readonly bool _continueOnCapturedContext;
    readonly WeakEventManager _weakEventManager = new WeakEventManager();

    public AsyncCommand(Func<T, Task> execute,
                        Func<object, bool> canExecute = null,
                        Action<Exception> onException = null,
                        bool continueOnCapturedContext = false)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute), $"{nameof(execute)} cannot be null");
        _canExecute = canExecute ?? (_ => true);
        _onException = onException;
        _continueOnCapturedContext = continueOnCapturedContext;
    }

    public event EventHandler CanExecuteChanged
    {
        add => _weakEventManager.AddEventHandler(value);
        remove => _weakEventManager.RemoveEventHandler(value);
    }

    public bool CanExecute(object parameter) => _canExecute(parameter);

    public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

    public Task ExecuteAsync(T parameter) =>
        _execute(parameter);

    void ICommand.Execute(object parameter)
    {
        switch (parameter)
        {
            case T validParameter:
                ExecuteAsync(validParameter).SafeFireAndForget(_continueOnCapturedContext, _onException);
                break;

            case null when !typeof(T).GetTypeInfo().IsValueType:
                ExecuteAsync((T)parameter).SafeFireAndForget(_continueOnCapturedContext, _onException);
                break;

            case null:
                throw new ArgumentException($"Parameter {parameter.GetType().Name} can not be converted to {typeof(T).Name}");

            default:
                throw new ArgumentException($"Parameter {parameter.GetType().Name} can not be converted to {typeof(T).Name}");
        }
    }
}

/// <summary>
/// Based on <see href="https://github.com/brminnick/AsyncAwaitBestPractices"/>
/// </summary>
public class AsyncCommand : IAsyncCommand
{
    readonly Func<Task> _execute;
    readonly Func<object, bool> _canExecute;
    readonly Action<Exception> _onException;
    readonly bool _continueOnCapturedContext;
    readonly WeakEventManager _weakEventManager = new WeakEventManager();

    public AsyncCommand(Func<Task> execute,
                        Func<object, bool> canExecute = null,
                        Action<Exception> onException = null,
                        bool continueOnCapturedContext = false)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute), $"{nameof(execute)} cannot be null");
        _canExecute = canExecute ?? (_ => true);
        _onException = onException;
        _continueOnCapturedContext = continueOnCapturedContext;
    }

    public event EventHandler CanExecuteChanged
    {
        add => _weakEventManager.AddEventHandler(value);
        remove => _weakEventManager.RemoveEventHandler(value);
    }

    public bool CanExecute(object parameter) => _canExecute(parameter);

    public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

    public Task ExecuteAsync() => _execute();

    void ICommand.Execute(object parameter) => _execute().SafeFireAndForget(_continueOnCapturedContext, _onException);
}
