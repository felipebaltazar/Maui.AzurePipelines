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

    Task<AzureApiResult<string>> GetLogAsync(
        string organization,
        string project,
        string buildId,
        string logId);

    Task<IEnumerable<Team>> GetTeamsAsync(
        string organization);

    Task<IEnumerable<Board>> GetBoardsAsync(
        string organization,
        string project,
        string teamId);

    Task<AzureApiResult<Project>> GetProjectsAsync(string organization);

    Task<(bool IsSuccess, string Message)> ApproveAsync(
        string organization,
        string project,
        params ApprovalUpdate[] update);

    Task<ImageApiResult> GetImageAsync(string url);
}
