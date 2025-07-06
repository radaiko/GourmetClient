using System;
using GourmetClient.Model;

namespace GourmetClient.Serialization;

internal class SerializableUserInformation
{
    public SerializableUserInformation()
    {
        // Used for deserialization
    }

    public SerializableUserInformation(GourmetUserInformation userInformation)
    {
        NameOfUser = userInformation.NameOfUser;
        ShopModelId = userInformation.ShopModelId;
        EaterId = userInformation.EaterId;
        StaffGroupId = userInformation.StaffGroupId;
    }

    public string? NameOfUser { get; set; }

    public string? ShopModelId { get; set; }

    public string? EaterId { get; set; }

    public string? StaffGroupId { get; set; }

    public GourmetUserInformation ToGourmetUserInformation()
    {
        if (NameOfUser is null || ShopModelId is null || EaterId is null || StaffGroupId is null)
        {
            throw new InvalidOperationException("Information for the user information is not complete");
        }

        return new GourmetUserInformation(NameOfUser, ShopModelId, EaterId, StaffGroupId);
    }
}