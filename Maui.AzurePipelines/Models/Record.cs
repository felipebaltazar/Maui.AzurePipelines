using PipelineApproval.Models;
using System.Text.Json.Serialization;

namespace PipelineApproval;

public class Record : ObservableObject
{
    private bool isExpanded;

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

    [JsonIgnore]
    public bool IsExpanded
    {
        get => true;// type == "Checkpoint" || type == "Phase" || isExpanded;
        set => SetProperty(ref isExpanded, value);
    }

    public Thickness GetMargin()
    {
        if (type == "Stage")
        {
            return Thickness.Zero;
        }
        else if(type == "Job" || type == "Checkpoint.Approval")
        {

            return new Thickness(16, 0, 0, 0);
        }

        return new Thickness(32, 0, 0, 0);
    }

    public string GetStateIcon()
    {
        if(result == "skipped")
        {
            return Icons.Skipped;
        }
        else if (state == "inProgress")
        {
            return Icons.Running;
        }
        else if (state == "notStarted" || state == "pending")
        {
            return Icons.Queued;
        }
        else if (state == "cancelling" || state == "cancelled")
        {
            return Icons.Cancelled;
        }
        else if (state == "completed")
        {
            return Icons.Success;
        }

        return Icons.Failed;
    }

    public Color GetStateColor()
    {
        if (state == "inProgress")
        {
            return Color.FromArgb("#0078d4");
        }
        else if (result == "skipped" || state == "notStarted" || state == "pending")
        {
            return Colors.White;
        }
        else if (state == "cancelling" || state == "cancelled")
        {
            return Color.FromArgb("#cd4a45");
        }
        else if (state == "completed")
        {
            return Color.FromArgb("#55a362");
        }

        return Color.FromArgb("#cd4a45");
    }
}
