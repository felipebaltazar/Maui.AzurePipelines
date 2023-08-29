using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineApproval.Models;


public class BuildOverview
{
    public _Links _links { get; set; }
    public object properties { get; set; }
    public string[] tags { get; set; }
    public object[] validationResults { get; set; }
    public Plan[] plans { get; set; }
    public Triggerinfo triggerInfo { get; set; }
    public int id { get; set; }
    public string buildNumber { get; set; }
    public string status { get; set; }
    public DateTime queueTime { get; set; }
    public DateTime startTime { get; set; }
    public string url { get; set; }
    public Definition definition { get; set; }
    public Project project { get; set; }
    public string uri { get; set; }
    public string sourceBranch { get; set; }
    public string sourceVersion { get; set; }
    public Queue queue { get; set; }
    public string priority { get; set; }
    public string reason { get; set; }
    public Requestedfor requestedFor { get; set; }
    public Requestedby requestedBy { get; set; }
    public DateTime lastChangedDate { get; set; }
    public Lastchangedby lastChangedBy { get; set; }
    public Orchestrationplan orchestrationPlan { get; set; }
    public Logs logs { get; set; }
    public Repository repository { get; set; }
    public bool retainedByRelease { get; set; }
    public object triggeredByBuild { get; set; }
    public bool appendCommitMessageToRunName { get; set; }
    public int buildNumberRevision { get; set; }
    public string queueOptions { get; set; }
    public Dictionary<string, string> templateParameters { get; set; }
    public string parameters { get; set; }


    public string GetStateIcon()
    {
        if(status == "inProgress")
        {
            return Icons.Running;
        }
        else if(status == "notStarted")
        {
            return Icons.Queued;
        }
        else if (status == "cancelling" || status == "cancelled")
        {
            return Icons.Cancelled;
        }
        else if (status == "completed")
        {
            return Icons.Success;
        }

        return Icons.Failed;
    }

    public Color GetStateColor()
    {
        if (status == "inProgress")
        {
            return Color.FromArgb("#0078d4");
        }
        else if (status == "notStarted")
        {
            return Colors.White;
        }
        else if (status == "cancelling" || status == "cancelled")
        {
            return Color.FromArgb("#cd4a45");
        }
        else if (status == "completed")
        {
            return Color.FromArgb("#55a362");
        }

        return Color.FromArgb("#cd4a45");
    }

    public string GetQeuetime()
    {
        var diff = (DateTime.UtcNow - queueTime);

        if (diff.Minutes == 0)
        {
            return "Now";
        }
        else if(diff.Hours == 0)
        {
            return diff.Minutes + " minutos atrás";
        }
        else if (diff.Days == 0)
        {
            return diff.Hours + " horas atrás";
        }

        return diff.Days + " dias atrás";
    }
}
