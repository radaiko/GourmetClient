using System.Text.Json.Serialization;

namespace GourmetClient.Network.GourmetApi;

internal class BillingItem
{
    [JsonPropertyName("Description")]
    public required string Description { get; set; }

    [JsonPropertyName("Count")]
    public required int Count { get; set; }

    [JsonPropertyName("Total")]
    public required double TotalCost { get; set; }

    [JsonPropertyName("Subsidy")]
    public required double Subsidy { get; set; }
}