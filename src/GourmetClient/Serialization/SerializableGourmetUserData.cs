namespace GourmetClient.Serialization
{
    using System;
    using Model;

    internal class SerializableUserInformation
    {
        public SerializableUserInformation()
        {
            // Used for deserialization
        }

        public SerializableUserInformation(GourmetUserInformation userInformation)
        {
            userInformation = userInformation ?? throw new ArgumentNullException(nameof(userInformation));

            NameOfUser = userInformation.NameOfUser;
            ShopModelId = userInformation.ShopModelId;
            EaterId = userInformation.EaterId;
            StaffGroupId = userInformation.StaffGroupId;
        }

        public string NameOfUser { get; set; }

        public string ShopModelId { get; set; }

        public string EaterId { get; set; }

        public string StaffGroupId { get; set; }

        public GourmetUserInformation ToGourmetUserInformation()
        {
            return new GourmetUserInformation(NameOfUser, ShopModelId, EaterId, StaffGroupId);
        }
    }
}
