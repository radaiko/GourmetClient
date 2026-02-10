using GourmetClient.Maui.Core.Model;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableGourmetCache
{
    public static SerializableGourmetCache FromGourmetCache(GourmetCache cache)
    {
        return new SerializableGourmetCache
        {
            Version = 2,
            Timestamp = cache.Timestamp,
            UserInformation = SerializableGourmetUserInformation.FromGourmetUserInformation(cache.UserInformation),
            Menus = cache.Menus.Select(SerializableGourmetMenu.FromGourmetMenu).ToArray(),
            OrderedMenus = cache.OrderedMenus.Select(SerializableGourmetOrderedMenu.FromGourmetOrderedMenu).ToArray()
        };
    }

    [JsonPropertyName("Version")]
    public required int Version { get; set; }

    [JsonPropertyName("Timestamp")]
    public required DateTime Timestamp { get; set; }

    [JsonPropertyName("UserInformation")]
    public required SerializableGourmetUserInformation UserInformation { get; set; }

    [JsonPropertyName("Menus")]
    public required SerializableGourmetMenu[] Menus { get; set; }

    [JsonPropertyName("OrderedMenus")]
    public required SerializableGourmetOrderedMenu[] OrderedMenus { get; set; }

    public GourmetCache ToGourmetMenuCache()
    {
        if (Version != 2)
        {
            throw new InvalidOperationException($"Unsupported version of serialized data: {Version}");
        }

        return new GourmetCache(
            Timestamp,
            UserInformation.ToGourmetUserInformation(),
            Menus.Select(serializedMenu => serializedMenu.ToGourmetMenu()).ToArray(),
            OrderedMenus.Select(serializedOrderedMenu => serializedOrderedMenu.ToOrderedGourmetMenu()).ToArray());
    }
}