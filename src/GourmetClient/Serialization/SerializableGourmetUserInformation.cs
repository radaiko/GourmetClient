using GourmetClient.Model;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableGourmetUserInformation
{
    public static SerializableGourmetUserInformation FromGourmetUserInformation(GourmetUserInformation userInformation)
    {
        return new SerializableGourmetUserInformation
        {
            NameOfUser = userInformation.NameOfUser,
            ShopModelId = userInformation.ShopModelId,
            EaterId = userInformation.EaterId,
            StaffGroupId = userInformation.StaffGroupId
        };
    }

    [JsonPropertyName("NameOfUser")]
    public required string NameOfUser { get; set; }

    [JsonPropertyName("ShopModelId")]
    public required string ShopModelId { get; set; }

    [JsonPropertyName("EaterId")]
    public required string EaterId { get; set; }

    [JsonPropertyName("StaffGroupId")]
    public required string StaffGroupId { get; set; }

    public GourmetUserInformation ToGourmetUserInformation()
    {
        return new GourmetUserInformation(NameOfUser, ShopModelId, EaterId, StaffGroupId);
    }
}