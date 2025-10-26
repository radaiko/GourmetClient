using System.Diagnostics;
using GC.Core.Cache;
using GC.Core.WebApis;
using GC.Models;
using GC.Tests.Helpers;

namespace GC.Tests.E2E.CacheTests;

[Collection("Sequential")]
public class BillingCacheTests {
  [Fact]
  public async Task Test1() {
    var tempDb = PathHelper.GetTempDbPath();
    try { File.Delete(tempDb); } catch { }
    SqliteCacheBase.SetDbPathForTests(tempDb);

    // Use cassette-capable HttpClient
    BaseApi.SetHttpClient(HttpCassette.CreateHttpClient());

    var months = new List<BillingMonth>();
    await foreach (var month in BillingCache.GetAsync()) {
      months.Add(month);
    }
    
    Assert.NotEmpty(months);
  }
}