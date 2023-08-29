namespace PipelineApproval.Models;

public class Repository
{
    public string id { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public object clean { get; set; }
    public bool checkoutSubmodules { get; set; }
}
