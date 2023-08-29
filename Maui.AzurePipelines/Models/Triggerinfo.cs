using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class Triggerinfo
{
    [JsonPropertyName("ci.sourceBranch")]
    public string cisourceBranch { get; set; }

    [JsonPropertyName("ci.sourceSha")]
    public string cisourceSha { get; set; }

    [JsonPropertyName("ci.message")]
    public string cimessage { get; set; }

    [JsonPropertyName("ci.triggerRepository")]
    public string citriggerRepository { get; set; }

    [JsonPropertyName("pr.number")]
    public string prnumber { get; set; }

    [JsonPropertyName("pr.isFork")]
    public string prisFork { get; set; }

    [JsonPropertyName("pr.triggerRepository")]
    public string prtriggerRepository { get; set; }

    [JsonPropertyName("pr.triggerRepositoryType")]
    public string prtriggerRepositoryType { get; set; }
}
