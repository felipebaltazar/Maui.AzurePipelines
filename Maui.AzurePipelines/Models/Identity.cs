using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class Identity
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("descriptor")]
    public string Descriptor { get; set; }

    [JsonPropertyName("subjectDescriptor")]
    public string SubjectDescriptor { get; set; }

    [JsonPropertyName("providerDisplayName")]
    public string ProviderDisplayName { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isContainer")]
    public bool IsContainer { get; set; }

    [JsonPropertyName("members")]
    public object[] Members { get; set; }

    [JsonPropertyName("memberOf")]
    public object[] MemberOf { get; set; }

    [JsonPropertyName("masterId")]
    public string MasterId { get; set; }

    [JsonPropertyName("resourceVersion")]
    public int ResourceVersion { get; set; }

    [JsonPropertyName("metaTypeId")]
    public int MetaTypeId { get; set; }
}
