using System;
using Semver;

namespace GourmetClient.Core.Update;

public record ReleaseDescription(
    SemVersion Version,
    string UpdatePackageDownloadUrl,
    long UpdatePackageSize,
    string ChecksumDownloadUrl,
    long ChecksumSize);