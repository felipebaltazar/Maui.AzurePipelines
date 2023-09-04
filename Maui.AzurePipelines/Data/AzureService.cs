using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Data;
using PipelineApproval.Infrastructure;
using PipelineApproval.Models;
using System.Security.Cryptography;
using System.Text;

namespace PipelineApproval;

public sealed class AzureService : BaseMicrosoftService, IAzureService
{
    #region Fields

    private readonly IAzureApi _azureApi;
    private readonly ILogger _logger;

    private readonly IDictionary<string, string> _continuationTokens =
        new Dictionary<string, string>();

    #endregion

    #region Constructors

    public AzureService(IAzureApi azureApi, ILogger logger)
    {
        _azureApi = azureApi;
        _logger = logger;
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

    public async Task<AzureApiResult<string>> GetLogAsync(
        string organization,
        string project,
        string buildId,
        string logId)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var log = await RequestWithRetryPolicy(() => _azureApi.GetLogAsync(credentials, organization, project, buildId, logId)).ConfigureAwait(false);

        return log;
    }

    public async Task<IEnumerable<Team>> GetTeamsAsync(string organization)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var team = await RequestWithRetryPolicy(() => _azureApi.GetTeamsAsync(credentials, organization)).ConfigureAwait(false);
        return team.value;
    }

    public async Task<IEnumerable<Board>> GetBoardsAsync(string organization, string project, string teamId)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var results = await RequestWithRetryPolicy(() =>
                _azureApi.GetBoardsAsync(credentials, organization, project, teamId));

        return results.value;
    }

    public async Task<(bool IsSuccess, string Message)> ApproveAsync(string organization, string project, params ApprovalUpdate[] update)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var result = await RequestWithRetryPolicy(() =>
                _azureApi.ApproveAsync(credentials, organization, project, update)).ConfigureAwait(false);

        var teste = result.Content;

        return (result.IsSuccessStatusCode, result.Content);
    }

    public async Task<AzureApiResult<Project>> GetProjectsAsync(string organization)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        var result = await RequestWithRetryPolicy(() =>
                _azureApi.GetProjectsAsync(credentials, organization)).ConfigureAwait(false);

        var imagesTasks = result.value.Select(p => GetImageFromProjectAsync(p));
        var httpResponses = await Task.WhenAll(imagesTasks).ConfigureAwait(false);
        var projects = httpResponses.Select(SaveImage);
        result.value = projects.ToArray();

        return result;
    }

    public async Task<ImageApiResult> GetImageAsync(string url)
    {
        var credentials = await GetCredentialsAsync().ConfigureAwait(false);
        return await RequestWithRetryPolicy(() => _azureApi.GetImageAsync(credentials, url)).ConfigureAwait(false);
    }

    #endregion

    #region Private Methods

    private async Task<(ImageApiResult, Project)> GetImageFromProjectAsync(Project p)
    {
        try
        {
            var baseUrl = $"{Constants.Url.AZURE_API}/";
            var finalUrl = p.defaultTeamImageUrl.Replace(baseUrl, string.Empty);
            var result = await GetImageAsync(finalUrl).ConfigureAwait(false);
            return (result, p);
        }
        catch (Exception)
        {
        }

        return (null, p);
    }

    private Project SaveImage((ImageApiResult image, Project project) result)
    {
        if (result.image is null)
            return result.project;

        try
        {
            var hash = GenerateHash(result.project.defaultTeamImageUrl);
            var cacheDir = Path.Combine(FileSystem.Current.CacheDirectory, "images");
            var finalPath = Path.Combine(cacheDir, hash + ".png");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            if (!File.Exists(finalPath))
            {
                File.WriteAllBytes(finalPath, Convert.FromBase64String(result.image.imageData));
            }

            result.project.TeamImageFile = finalPath;
            return result.project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possivel baixar imagem");
        }

        return result.project;
    }

    private string GenerateHash(string text)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));

            var hexChars = bytes
                .Select(b => b.ToString("X2"));

            return string.Concat(hexChars);
        }
    }

    #endregion
}
