namespace PipelineApproval.Abstractions;

public interface IBrowserService
{
    public Task OpenAsync(Uri uriToOpen);
}
