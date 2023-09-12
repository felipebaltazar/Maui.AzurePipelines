using System.Text.Json.Serialization;

namespace PipelineApproval.Models;


public class UserEntitlementsReponseApi
{
    [JsonPropertyName("members")]
    public Member[] Members { get; set; }

    [JsonPropertyName("continuationToken")]
    public string ContinuationToken { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("items")]
    public Member[] Items { get; set; }
}
