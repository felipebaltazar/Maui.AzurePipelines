using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class User
{
    [JsonPropertyName("subjectKind")]
    public string SubjectKind { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("principalName")]
    public string PrincipalName { get; set; }

    [JsonPropertyName("mailAddress")]
    public string MailAddress { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    [JsonPropertyName("originId")]
    public string OriginId { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("_links")]
    public _Links Links { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }
}