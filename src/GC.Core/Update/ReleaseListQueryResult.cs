using System.Collections.Generic;

namespace GC.Core.Update;

public record ReleaseListQueryResult(
  string ETagHeaderValue,
  bool IsWeakETag,
  IReadOnlyCollection<ReleaseDescription> Releases);