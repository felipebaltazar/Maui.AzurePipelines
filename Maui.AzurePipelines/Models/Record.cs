namespace PipelineApproval;

public class Record
{
    public object[] previousAttempts { get; set; }
    public string id { get; set; }
    public string parentId { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public DateTime? startTime { get; set; }
    public DateTime? finishTime { get; set; }
    public object currentOperation { get; set; }
    public int? percentComplete { get; set; }
    public string state { get; set; }
    public string result { get; set; }
    public string resultCode { get; set; }
    public int changeId { get; set; }
    public DateTime lastModified { get; set; }
    public string workerName { get; set; }
    public int order { get; set; }
    public Details details { get; set; }
    public int errorCount { get; set; }
    public int warningCount { get; set; }
    public object url { get; set; }
    public Log log { get; set; }
    public PipelineTask task { get; set; }
    public int attempt { get; set; }
    public string identifier { get; set; }
    public int queueId { get; set; }
    public Issue[] issues { get; set; }
}
