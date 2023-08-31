using PipelineApproval.Models;
using Refit;

namespace PipelineApproval.Abstractions.Data;

[Headers("Accept:application/json")]
public interface IVisualStudioApi
{
    [Get("/_apis/profile/profiles/me?api-version={apiVersion}")]
    Task<AccountInfo> GetAccountInfoAsync(
        [Header("Authorization")] string authentication,
        string apiVersion = "5.1");

    [Get("/_apis/accounts?api-version={apiVersion}&memberId={userId}")]
    Task<AccountResponseApi> GetOrganizationsAsync(
        [Header("Authorization")] string authentication,
        string userId,
        string apiVersion = "5.1");
}
