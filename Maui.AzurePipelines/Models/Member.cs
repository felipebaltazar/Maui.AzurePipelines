using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class Member
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("extensions")]
    public object[] Extensions { get; set; }

    [JsonPropertyName("accessLevel")]
    public Accesslevel AccessLevel { get; set; }

    [JsonPropertyName("dateCreated")]
    public DateTime DateCreated { get; set; }

    [JsonPropertyName("lastAccessedDate")]
    public DateTime LastAccessedDate { get; set; }

    [JsonPropertyName("projectEntitlements")]
    public object[] ProjectEntitlements { get; set; }

    [JsonPropertyName("groupAssignments")]
    public object[] GroupAssignments { get; set; }
}
