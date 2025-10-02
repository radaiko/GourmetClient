using System.Collections.Generic;
using GourmetClient.Core.Model;

namespace GourmetClient.Core.Update;

public record ReleaseListQueryResult(
    string ETagHeaderValue,
    bool IsWeakETag,
    IReadOnlyCollection<ReleaseDescription> Releases);