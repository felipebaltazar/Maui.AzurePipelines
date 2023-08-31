namespace PipelineApproval.Abstractions;

public interface ISecureStorageService
{
    Task<string> GetAsync(string key);
}
