using Semver;

namespace GC.Core.Update;

public record ReleaseDescription(
    SemVersion Version,
    string UpdatePackageDownloadUrl,
    long UpdatePackageSize,
    string ChecksumDownloadUrl,
    long ChecksumSize);