namespace PipelineApproval.Models;

public class Definition
{
    public object[] drafts { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string uri { get; set; }
    public string path { get; set; }
    public string type { get; set; }
    public string queueStatus { get; set; }
    public int revision { get; set; }
    public Project project { get; set; }
}
