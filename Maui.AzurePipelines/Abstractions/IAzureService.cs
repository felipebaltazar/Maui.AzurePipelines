using PipelineApproval.Models;

namespace PipelineApproval.Abstractions;

public interface IAzureService
{
    void ClearCache();

    Task<AzureApiResult<BuildOverview>> GetBuildsAsync(
        string organization,
        string project,
        int top = 100,
        string queryOrder = "queueTimeDescending");
}
