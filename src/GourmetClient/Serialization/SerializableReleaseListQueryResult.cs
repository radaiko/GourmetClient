using GourmetClient.Update;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableReleaseListQueryResult
{
    public static SerializableReleaseListQueryResult FromReleaseListQueryResult(ReleaseListQueryResult releaseListQueryResult)
    {
        return new SerializableReleaseListQueryResult
        {
            Version = 1,
            ETagHeaderValue = releaseListQueryResult.ETagHeaderValue,
            IsWeakETag = releaseListQueryResult.IsWeakETag,
            Releases = releaseListQueryResult.Releases.Select(SerializableReleaseDescription.FromReleaseDescription).ToArray()
        };
    }

    [JsonPropertyName("Version")]
    public required int Version { get; set; }

    [JsonPropertyName("ETagHeaderValue")]
    public required string ETagHeaderValue { get; set; }

    [JsonPropertyName("IsWeakETag")]
    public required bool IsWeakETag { get; set; }

    [JsonPropertyName("Releases")]
    public required SerializableReleaseDescription[] Releases { get; set; }

    public ReleaseListQueryResult ToReleaseListQueryResult()
    {
        if (Version is not 1)
        {
            throw new InvalidOperationException($"Unsupported version of serialized data: {Version}");
        }

        return new ReleaseListQueryResult(
            ETagHeaderValue, 
            IsWeakETag,
            Releases.Select(release => release.ToReleaseDescription()).ToArray());
    }
}