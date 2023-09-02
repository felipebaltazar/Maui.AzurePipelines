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
}
