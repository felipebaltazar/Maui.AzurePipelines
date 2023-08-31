namespace PipelineApproval.Abstractions
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(string name, string baseUrl = null, bool shouldLogHttpResponseContent = true);

        HttpMessageHandler CreateHandler(string name, bool shouldLogHttpResponseContent = true);
    }
}
