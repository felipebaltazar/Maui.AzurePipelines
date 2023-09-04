using System.Text.Json.Serialization;

namespace PipelineApproval.Models;

public class Team
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("identityUrl")]
    public string IdentityUrl { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; }

    [JsonPropertyName("identity")]
    public Identity Identity { get; set; }
}
