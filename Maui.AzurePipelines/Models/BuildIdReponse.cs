namespace PipelineApproval;

public class BuildIdReponse
{
    public Record[] records { get; set; }
    public string lastChangedBy { get; set; }
    public DateTime lastChangedOn { get; set; }
    public string id { get; set; }
    public int changeId { get; set; }
    public string url { get; set; }
}
