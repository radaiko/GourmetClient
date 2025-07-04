namespace GourmetClient.Serialization;

using System;
using System.Linq;
using Model;

internal class SerializableGourmetCache
{
    public SerializableGourmetCache()
    {
        // Used for deserialization
        Timestamp = DateTime.MinValue;
        Menus = [];
        OrderedMenus = [];
    }

    public SerializableGourmetCache(GourmetCache menuCache)
    {
        menuCache = menuCache ?? throw new ArgumentNullException(nameof(menuCache));

        Version = 2;
        Timestamp = menuCache.Timestamp;
        UserInformation = new SerializableUserInformation(menuCache.UserInformation);
        Menus = menuCache.Menus.Select(menu => new SerializableGourmetMenu(menu)).ToArray();
        OrderedMenus = menuCache.OrderedMenus.Select(orderedMenu => new SerializableGourmetOrderedMenu(orderedMenu)).ToArray();
    }

    public int? Version { get; set; }

    public DateTime Timestamp { get; set; }

    public SerializableUserInformation UserInformation { get; set; }

    public SerializableGourmetMenu[] Menus { get; set; }

    public SerializableGourmetOrderedMenu[] OrderedMenus { get; set; }

    public GourmetCache ToGourmetMenuCache()
    {
        return new GourmetCache(
            Timestamp,
            UserInformation.ToGourmetUserInformation(),
            Menus.Select(serializedMenu => serializedMenu.ToGourmetMenu()).ToArray(),
            OrderedMenus.Select(serializedOrderedMenu => serializedOrderedMenu.ToOrderedGourmetMenu()).ToArray());
    }
}