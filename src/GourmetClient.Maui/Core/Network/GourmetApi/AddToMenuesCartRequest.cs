using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Network.GourmetApi;

internal class AddToMenuesCartRequest
{
    [JsonPropertyName("eaterId")]
    public required string EaterId { get; set; }

    [JsonPropertyName("shopModelId")]
    public required string ShopModelId { get; set; }

    [JsonPropertyName("staffgroupId")]
    public required string StaffGroupId { get; set; }

    [JsonPropertyName("dates")]
    public required AddToMenuesCartDate[] Dates { get; set; }
}