using System.Windows.Input;

namespace PipelineApproval.Abstractions;

public interface IAsyncCommand<T> : ICommand
{
    Task ExecuteAsync(T parameter);

    void RaiseCanExecuteChanged();
}

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();

    void RaiseCanExecuteChanged();
}
