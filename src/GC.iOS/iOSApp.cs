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
    }

    protected override void ConfigureServices(ServiceCollection services)
    {
        base.ConfigureServices(services);
        // Platform-specific services
        services.AddSingleton<IFilePathProvider, FilePathProvider>();
        services.AddSingleton<IThemeService, iOSThemeService>();
    }

    protected override IThemeService? GetPlatformThemeService()
    {
        return ServiceProviderHolder.Services.GetRequiredService<IThemeService>();
    }
}
