using PipelineApproval.Models;

namespace PipelineApproval.Abstractions;

public interface IVisualStudioService
{
    Task<bool> ValidateAccessTokenAsync(string pat);

    Task<AccountResponseApi> GetOrganizationsAsync();
}
