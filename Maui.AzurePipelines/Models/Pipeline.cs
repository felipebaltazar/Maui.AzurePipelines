namespace PipelineApproval;

public class Pipeline
{
    public Owner owner { get; set; }
    public string id { get; set; }
    public string name { get; set; }
}

public class PipelineOverview
{
    public int id { get; set; }
    public string name { get; set; }
    public _Links _links { get; set; }
    public string url { get; set; }
    public int revision { get; set; }
    public string folder { get; set; }
}
