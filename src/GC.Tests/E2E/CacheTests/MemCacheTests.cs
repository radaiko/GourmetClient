using GC.Core.Cache;
using GC.Core.DB;

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

public class MemCacheTests(MemCacheTestsFixture fixture) : IClassFixture<MemCacheTestsFixture>
{
    [Fact]
    public async Task MemCache_Can_Be_Initialized()
    {
        await MemCache.Initialize();

        Assert.NotNull(MemCache.BillingMonths);
        Assert.NotEmpty(MemCache.BillingMonths);
        Assert.NotNull(MemCache.Menus);
        Assert.NotEmpty(MemCache.Menus);
    }

    [Fact]
    public async Task MemCache_Can_Refresh_Billing_Months()
    {
        await MemCache.RefreshBillingMonthsAsync();

        Assert.NotNull(MemCache.BillingMonths);
        Assert.NotEmpty(MemCache.BillingMonths);
    }

    [Fact]
    public async Task MemCache_Can_Refresh_Order_Days()
    {
        await MemCache.RefreshOrderDaysAsync();

        Assert.NotNull(MemCache.Menus);
        Assert.NotEmpty(MemCache.Menus);
    }
}