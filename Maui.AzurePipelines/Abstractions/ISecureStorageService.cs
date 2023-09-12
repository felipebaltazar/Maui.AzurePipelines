namespace PipelineApproval.Abstractions;

public interface ISecureStorageService
{
    Task<string> GetAsync(string key);

    Task SetAsync(string key, string value);
}
