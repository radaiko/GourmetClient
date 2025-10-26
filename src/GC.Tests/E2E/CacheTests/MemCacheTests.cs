using GC.Core.Cache;
using GC.Core.DB;
using GC.Core.WebApis;
using GC.Tests.Helpers;

namespace GC.Tests.E2E.CacheTests;

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class MemCacheTestsFixture : IDisposable {
    public string TempDb { get; }

    public MemCacheTestsFixture() {
        TempDb = Helpers.PathHelper.GetTempDbPath();
        try {
            File.Delete(TempDb);
        }
        catch {
            // ignored
        }
        SQLiteBase.DbPath = TempDb;
    }

    public void Dispose() {
        SQLiteBase.Close();
    }
}

[Collection("Sequential")]
public class MemCacheTests(MemCacheTestsFixture fixture) : IClassFixture<MemCacheTestsFixture>
{
    [Fact]
    public async Task MemCache_Can_Be_Initialized()
    {
        // Set up HttpClient with cassette
        BaseApi.SetHttpClient(HttpCassette.CreateHttpClient());
        await MemCache.Initialize();

        Assert.NotNull(MemCache.BillingMonths);
        Assert.NotEmpty(MemCache.BillingMonths);
        Assert.NotNull(MemCache.Menus);
        Assert.NotEmpty(MemCache.Menus);
    }

    [Fact]
    public async Task MemCache_Can_Refresh_Billing_Months()
    {
        // Set up HttpClient with cassette
        BaseApi.SetHttpClient(HttpCassette.CreateHttpClient());
        await MemCache.RefreshBillingMonthsAsync();

        Assert.NotNull(MemCache.BillingMonths);
        Assert.NotEmpty(MemCache.BillingMonths);
    }

    [Fact]
    public async Task MemCache_Can_Refresh_Order_Days()
    {
        // Set up HttpClient with cassette
        BaseApi.SetHttpClient(HttpCassette.CreateHttpClient());
        await MemCache.RefreshOrderDaysAsync();

        Assert.NotNull(MemCache.Menus);
        Assert.NotEmpty(MemCache.Menus);
    }
}