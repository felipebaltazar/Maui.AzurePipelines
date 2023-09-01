using PipelineApproval.Models;

namespace PipelineApproval.Abstractions;

public interface IAzureService
{
    void ClearCache();

    Task<BuildOverview> GetBuildAsync(
        string organization,
        string project,
        string buildId);

    Task<AzureApiResult<BuildOverview>> GetBuildsAsync(
        string organization,
        string project,
        int top = 100,
        string queryOrder = "queueTimeDescending");

    Task<(IEnumerable<Approval> Approvals, IEnumerable<Record> Records)> GetBuildTimeLineAsync(
        string organization,
        string project,
        int id);
}
