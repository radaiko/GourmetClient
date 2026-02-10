using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Network.GourmetApi;

internal class AddToMenuesCartResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}