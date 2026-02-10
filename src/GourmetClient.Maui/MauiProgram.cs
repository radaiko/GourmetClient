using Microsoft.Extensions.Logging;
using GourmetClient.Maui.Core.Network;
using GourmetClient.Maui.Core.Notifications;
using GourmetClient.Maui.Core.Settings;
using GourmetClient.Maui.Services;
using GourmetClient.Maui.Services.Implementations;
using GourmetClient.Maui.ViewModels;
using GourmetClient.Maui.Pages;

namespace GourmetClient.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if WINDOWS || MACCATALYST
        // Initialize Velopack for desktop update handling
        Velopack.VelopackApp.Build().Run();
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Platform abstractions
        services.AddSingleton<IAppDataPaths, MauiAppDataPaths>();
        services.AddSingleton<ICredentialService, AesCredentialService>();

        // Update service - platform specific
#if WINDOWS || MACCATALYST
        services.AddSingleton<IUpdateService, VelopackUpdateService>();
#else
        services.AddSingleton<IUpdateService, NoOpUpdateService>();
#endif

        // Core services (from WPF project, now with DI)
        services.AddSingleton<NotificationService>();
        services.AddSingleton<GourmetSettingsService>();
        services.AddSingleton<GourmetWebClient>();
        services.AddSingleton<VentopayWebClient>();
        services.AddSingleton<GourmetCacheService>();
        services.AddSingleton<BillingCacheService>();

        // ViewModels
        services.AddTransient<MenusViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<BillingViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Pages
        services.AddTransient<MenusPage>();
        services.AddTransient<OrdersPage>();
        services.AddTransient<BillingPage>();
        services.AddTransient<SettingsPage>();
    }
}
