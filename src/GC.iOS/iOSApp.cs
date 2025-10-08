using Avalonia.Svg.Skia;
using GC.Core.Network;
using GC.Core.Settings;
using GC.Core.Utils;
using GC.iOS.Services;
using GC.ViewModels;
using GC.ViewModels.Services;
using GC.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GC.iOS;

public class iOSApp : App
{
    public iOSApp()
    {
        // Force-link Svg types (helps prevent trimming on iOS)
        _ = typeof(SvgImage);
        _ = typeof(Avalonia.Svg.Skia.Svg);
        // Initialize dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        ServiceProviderHolder.Initialize(serviceProvider);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core services
        services.AddSingleton<IFilePathProvider, FilePathProvider>();
        services.AddSingleton<GourmetSettingsService>();
        
        // Network clients
        services.AddSingleton<GourmetWebClient>();
        services.AddSingleton<VentopayWebClient>();
        
        // ViewModels
        services.AddSingleton<MenuViewModel>();
        services.AddSingleton<BillingViewModel>();
        services.AddSingleton<MainViewModel>();
        
        // Platform services
        services.AddSingleton<IThemeService, iOSThemeService>();
    }

    protected override IThemeService? GetPlatformThemeService()
    {
        return ServiceProviderHolder.Services.GetRequiredService<IThemeService>();
    }
}
