namespace PipelineApproval.Abstractions;

public interface IApplicationLifecycleListener
{
    public Task OnResumedAsync();

    public Task OnPausedAsync();
}

