using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Data;
using PipelineApproval.Models;

namespace PipelineApproval;

public sealed class AzureService : BaseMicrosoftService, IAzureService
{
    #region Fields

    private readonly IAzureApi _azureApi;

    private readonly IDictionary<string, string> _continuationTokens =
        new Dictionary<string, string>();

    #endregion

    #region Constructors

    public AzureService(IAzureApi azureApi)
    {
        _azureApi = azureApi;
    }

    #endregion

    #region IAzureService

    public void ClearCache()
    {
        _continuationTokens?.Clear();
    }

    public async Task<BuildOverview> GetBuildAsync(
        string organization,
        string project,
        string buildId)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var result = await RequestWithRetryPolicy(() =>
            _azureApi.GetBuildAsync(
                credentials,
                organization,
                project,
                buildId)).ConfigureAwait(false);

        return result;
    }

    public async Task<AzureApiResult<BuildOverview>> GetBuildsAsync(
        string organization,
        string project,
        int top = 100,
        string queryOrder = "queueTimeDescending")
    {
        var continuationToken = string.Empty;
        _ = _continuationTokens.TryGetValue(nameof(GetBuildsAsync), out continuationToken);

        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var result = await RequestWithRetryPolicy(() =>
            _azureApi.GetBuildsAsync(
                credentials,
                organization,
                project,
                continuationToken,
                top,
                queryOrder)).ConfigureAwait(false);

        if (result.Headers.TryGetValues("x-ms-continuationtoken", out var values))
        {
            continuationToken = values.FirstOrDefault();
            if (!_continuationTokens.TryAdd(nameof(GetBuildsAsync), continuationToken))
            {
                _continuationTokens[nameof(GetBuildsAsync)] = continuationToken;
            }
        }

        return result.Content;
    }

    public async Task<(IEnumerable<Approval> Approvals, IEnumerable<Record> Records)> GetBuildTimeLineAsync(
        string organization,
        string project,
        int id)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var timeline = await _azureApi.GetBuildTimeLineAsync(credentials, organization, project, id.ToString()).ConfigureAwait(false);
        var tasks = timeline.records.Where(r => r.type == "Checkpoint.Approval")
                                    .Select(r => _azureApi.GetApprovalAsync(credentials, organization, project, r.id));

        var approvals = await Task.WhenAll(tasks).ConfigureAwait(false);

        return (approvals, timeline.records);
    }

    #endregion
}
