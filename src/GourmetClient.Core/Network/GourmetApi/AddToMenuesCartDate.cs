using System.Text.Json.Serialization;

namespace GourmetClient.Network.GourmetApi;

internal class AddToMenuesCartDate
{
    [JsonPropertyName("date")]
    public required string DateString { get; set; }

    [JsonPropertyName("menuIds")]
    public required string[] MenuIds { get; set; }
}