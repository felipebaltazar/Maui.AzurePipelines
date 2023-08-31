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
}
