using PipelineApproval.Abstractions;

namespace PipelineApproval.Infrastructure.Services;

public class SecureStorageService : ISecureStorageService
{
    public Task<string> GetAsync(string key) =>
        SecureStorage.GetAsync(key);
}
