using PipelineApproval.Models;
using Refit;

namespace PipelineApproval.Abstractions.Data;

[Headers("Accept:application/json")]
public interface IAzureApi
{
    [Get("/{organization}/{project}/_apis/build/builds?api-version={apiVersion}&$top={top}&queryOrder={queryOrder}&continuationToken={continuationToken}")]
    Task<ApiResponse<AzureApiResult<BuildOverview>>> GetBuildsAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string project,
        string continuationToken = null,
        int top = 100,
        string queryOrder = "queueTimeDescending",
        string apiVersion = "7.0");

    [Get("/{organization}/{project}/_apis/build/builds/{buildId}?api-version={apiVersion}")]
    Task<BuildOverview> GetBuildAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string project,
        string buildId,
        string apiVersion = "7.0");

    [Get("/{organization}/{project}/_apis/build/builds/{buildId}/timeline?api-version={apiVersion}")]
    Task<BuildIdReponse> GetBuildTimeLineAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string project,
        string buildId,
        string apiVersion = "6.0");

    [Get("/{organization}/{project}/_apis/pipelines/approvals/{recordId}?$expand=steps&$expand=permissions&api-version={apiVersion}")]
    Task<Approval> GetApprovalAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string project,
        string recordId,
        string apiVersion = "7.0-preview.1");

    [Get("/{organization}/{project}/_apis/build/builds/{buildId}/logs/{logId}")]
    Task<AzureApiResult<string>> GetLogAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string project,
        string buildId,
        string logId,
        string apiVersion = "7.0");

    [Get("/{organization}/_apis/teams?$mine=True&$expandIdentity=true&api-version={apiVersion}")]
    Task<AzureApiResult<Team>> GetTeamsAsync(
       [Header("Authorization")] string authentication,
       string organization,
       string apiVersion = "7.0-preview.3");

    [Get("/{organization}/{project}/{team}/_apis/work/boards?api-version={apiVersion}")]
    Task<AzureApiResult<Board>> GetBoardsAsync(
       [Header("Authorization")] string authentication,
       string organization,
       string project,
       string team,
       string apiVersion = "7.0");

    [Get("/{organization}/_apis/projects?api-version={apiVersion}&getDefaultTeamImageUrl={getTeamImage}")]
    Task<AzureApiResult<Project>> GetProjectsAsync(
        [Header("Authorization")] string authentication,
        string organization,
        bool getTeamImage = true,
        string apiVersion = "7.0");

    [Patch("/{organization}/{project}/_apis/pipelines/approvals?api-version={apiVersion}")]
    Task<ApiResponse<string>> ApproveAsync(
       [Header("Authorization")] string authentication,
       string organization,
       string project,
       [Body] ApprovalUpdate[] approvals,
       string apiVersion = "7.1-preview.1");


    [Get("/{imageUrl}")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<ImageApiResult> GetImageAsync(
       [Header("Authorization")] string authentication,
       string imageUrl);

}
