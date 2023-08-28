namespace PipelineApproval.Abstractions;

public interface IMainThreadService
{
    bool IsMainThread { get; }

    void BeginInvokeOnMainThread(Action action);
}
