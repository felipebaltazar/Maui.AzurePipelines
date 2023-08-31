namespace PipelineApproval.Models;

public struct ActiveHandlerTrackingEntry
{
    public ActiveHandlerTrackingEntry(string name, HttpMessageHandler handler)
    {
        Name = name;
        Handler = handler;
    }

    public HttpMessageHandler Handler { get; private set; }

    public string Name { get; }
}
