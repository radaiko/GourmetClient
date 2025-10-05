using GC.Desktop.Services;
using GC.ViewModels.Services;
using GC.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GourmetClient.Core.Settings;
using GourmetClient.Core.Utils;

namespace GC.Desktop;

public class DesktopApp : App
{
    public DesktopApp()
    {
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
        
        // Platform services
        services.AddSingleton<IThemeService, DesktopThemeService>();
    }

    protected override IThemeService? GetPlatformThemeService()
    {
        return ServiceProviderHolder.Services.GetRequiredService<IThemeService>();
    }
}
