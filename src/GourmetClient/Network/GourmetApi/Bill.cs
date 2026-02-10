using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Network.GourmetApi;

internal class Bill
{
    [JsonPropertyName("BillDate")]
    public required DateTime BillDate { get; set; }

    [JsonPropertyName("BillingItemInfo")]
    public required BillingItem[] BillingItems { get; set; }
}