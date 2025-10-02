using System.Text.Json.Serialization;

namespace GourmetClient.Update.GitHubApi;

internal class ReleaseEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("draft")]
    public required bool IsDraft { get; set; }

    [JsonPropertyName("assets")]
    public ReleaseAsset[] Assets { get; set; } = [];
}