namespace PipelineApproval.Abstractions
{
    public interface IInitializeAware
    {
        Task InitializeAsync(IDictionary<string, string> parameters);
    }
}
