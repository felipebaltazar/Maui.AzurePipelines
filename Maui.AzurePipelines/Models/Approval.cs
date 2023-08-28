using PipelineApproval.Abstractions;
using System.Windows.Input;

namespace PipelineApproval;

public class Approval
{
    public Record stageRecord { get; set; }

    public string id { get; set; }
    public Step[] steps { get; set; }
    public string status { get; set; }
    public DateTime createdOn { get; set; }
    public DateTime lastModifiedOn { get; set; }
    public string executionOrder { get; set; }
    public int minRequiredApprovers { get; set; }
    public object[] blockedApprovers { get; set; }
    public string permissions { get; set; }
    public _Links _links { get; set; }
    public Pipeline pipeline { get; set; }
    public string Comment { get; set; }
    public IAsyncCommand<Approval> ApproveCommand { get; set; }
}
