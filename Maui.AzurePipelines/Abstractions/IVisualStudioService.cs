using PipelineApproval.Models;

namespace PipelineApproval.Abstractions;

public interface IVisualStudioService
{
    Task<bool> ValidateAccessTokenAsync(string pat);

    Task<AccountResponseApi> GetOrganizationsAsync();

    Task<UserEntitlementsReponseApi> GetEntitlementsAsync(string pat, string organization);
}