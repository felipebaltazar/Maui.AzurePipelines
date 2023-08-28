namespace PipelineApproval;

public class Step
{
    public Assignedapprover assignedApprover { get; set; }
    public string status { get; set; }
    public DateTime lastModifiedOn { get; set; }
    public int order { get; set; }
    public Lastmodifiedby lastModifiedBy { get; set; }
    public DateTime initiatedOn { get; set; }
    public object[] history { get; set; }
}
