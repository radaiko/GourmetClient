using GourmetClient.Update;
using Semver;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableReleaseDescription
{
    public static SerializableReleaseDescription FromReleaseDescription(ReleaseDescription releaseDescription)
    {
        return new SerializableReleaseDescription
        {
            ReleaseVersion = releaseDescription.Version.ToString(),
            UpdatePackageDownloadUrl = releaseDescription.UpdatePackageDownloadUrl,
            UpdatePackageSize = releaseDescription.UpdatePackageSize,
            ChecksumDownloadUrl = releaseDescription.ChecksumDownloadUrl,
            ChecksumSize = releaseDescription.ChecksumSize
        };
    }

    [JsonPropertyName("ReleaseVersion")]
    public required string ReleaseVersion { get; set; }

    [JsonPropertyName("UpdatePackageDownloadUrl")]
    public required string UpdatePackageDownloadUrl { get; set; }

    [JsonPropertyName("UpdatePackageSize")]
    public required long UpdatePackageSize { get; set; }

    [JsonPropertyName("ChecksumDownloadUrl")]
    public required string ChecksumDownloadUrl { get; set; }

    [JsonPropertyName("ChecksumSize")]
    public required long ChecksumSize { get; set; }

    public ReleaseDescription ToReleaseDescription()
    {
        return new ReleaseDescription(
            SemVersion.Parse(ReleaseVersion, SemVersionStyles.Strict),
            UpdatePackageDownloadUrl,
            UpdatePackageSize,
            ChecksumDownloadUrl,
            ChecksumSize);
    }
}