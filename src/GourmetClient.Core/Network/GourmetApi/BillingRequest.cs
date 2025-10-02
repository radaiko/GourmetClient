using System.Text.Json.Serialization;

namespace GourmetClient.Network.GourmetApi;

internal class BillingRequest
{
    [JsonPropertyName("eaterId")]
    public required string EaterId { get; set; }

    [JsonPropertyName("shopModelId")]
    public required string ShopModelId { get; set; }

    /// <summary>
    /// Gets or sets the target month by specifying how many months back the report should be generated.
    /// This value starts at zero, i.e.,"0" means the current month, "1" means one month back, etc.
    /// </summary>
    [JsonPropertyName("checkLastMonthNumber")]
    public required string CheckLastMonthNumber { get; set; }
}