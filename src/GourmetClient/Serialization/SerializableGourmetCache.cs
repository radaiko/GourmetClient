using System;
using System.Linq;
using GourmetClient.Model;

namespace GourmetClient.Serialization;

internal class SerializableGourmetCache
{
    public SerializableGourmetCache()
    {
        // Used for deserialization
    }

    public SerializableGourmetCache(GourmetCache menuCache)
    {
        Version = 2;
        Timestamp = menuCache.Timestamp;
        UserInformation = new SerializableUserInformation(menuCache.UserInformation);
        Menus = menuCache.Menus.Select(menu => new SerializableGourmetMenu(menu)).ToArray();
        OrderedMenus = menuCache.OrderedMenus.Select(orderedMenu => new SerializableGourmetOrderedMenu(orderedMenu)).ToArray();
    }

    public int? Version { get; set; }

    public DateTime? Timestamp { get; set; }

    public SerializableUserInformation? UserInformation { get; set; }

    public SerializableGourmetMenu[]? Menus { get; set; }

    public SerializableGourmetOrderedMenu[]? OrderedMenus { get; set; }

    public GourmetCache ToGourmetMenuCache()
    {
        if (Version != 2)
        {
            throw new InvalidOperationException($"Unsupported version of serialized data: {Version}");
        }

        if (UserInformation == null)
        {
            throw new InvalidOperationException("UserInformation is missing");
        }

        return new GourmetCache(
            Timestamp ?? DateTime.MinValue,
            UserInformation.ToGourmetUserInformation(),
            Menus?.Select(serializedMenu => serializedMenu.ToGourmetMenu()).ToArray() ?? [],
            OrderedMenus?.Select(serializedOrderedMenu => serializedOrderedMenu.ToOrderedGourmetMenu()).ToArray() ?? []);
    }
}