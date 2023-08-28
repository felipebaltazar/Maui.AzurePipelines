using PipelineApproval.Abstractions;

namespace PipelineApproval.Infrastructure.Services;

public class BrowserService : IBrowserService
{
    public Task OpenAsync(Uri uriToOpen) =>
        Browser.Default.OpenAsync(uriToOpen);
}
