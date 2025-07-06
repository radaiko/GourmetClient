using System.Text.Json.Serialization;

namespace GourmetClient.Update.GitHubApi;

internal class ReleaseAsset
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public required string DownloadUrl { get; set; }

    [JsonPropertyName("size")]
    public required long Size { get; set; }
}