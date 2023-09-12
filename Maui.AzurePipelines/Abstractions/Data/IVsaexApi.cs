using PipelineApproval.Models;
using Refit;

namespace PipelineApproval.Abstractions.Data;

[Headers("Accept:application/json")]
public interface IVsaexApi
{
    [Get("/{organization}/_apis/userentitlements?api-version={apiVersion}")]
    Task<UserEntitlementsReponseApi> GetEntitlementsAsync(
        [Header("Authorization")] string authentication,
        string organization,
        string apiVersion = "5.1");
}
