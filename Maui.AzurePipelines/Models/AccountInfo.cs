namespace PipelineApproval.Models;

public class AccountInfo
{
    public string accountId { get; set; }
    public string accountUri { get; set; }
    public string accountName { get; set; }
    public object properties { get; set; }

    public string displayName { get; set; }
    public string publicAlias { get; set; }
    public string emailAddress { get; set; }
    public int coreRevision { get; set; }
    public DateTime timeStamp { get; set; }
    public string id { get; set; }
    public int revision { get; set; }
}
