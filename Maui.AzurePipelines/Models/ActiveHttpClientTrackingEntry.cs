namespace PipelineApproval.Models;

public struct ActiveHttpClientTrackingEntry
{
    public ActiveHttpClientTrackingEntry(string name, HttpClient client)
    {
        Name = name;
        Client = client;
    }

    public HttpClient Client { get; private set; }

    public string Name { get; }
}
