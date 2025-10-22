using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System;
using System.Runtime.InteropServices;

namespace GC.Frontend.Mobile;

public static class MauiProgramExtensions {
  public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder) {
    // call UseMauiApp first
    builder.UseMauiApp<App>();

    // UseMauiCommunityToolkit is platform-specific. Guard the call with runtime checks
    // so the analyzer (CA1416) doesn't warn about unsupported platforms. We also
    // locally suppress CA1416 because we already validate platforms at runtime.
    if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsWindows() || RuntimeInformation.IsOSPlatform(OSPlatform.Create("Tizen"))) {
#pragma warning disable CA1416 // Validate platform compatibility
      builder.UseMauiCommunityToolkit();
#pragma warning restore CA1416 // Validate platform compatibility
    }

    // configure fonts (kept separate from the chained calls above)
    builder.ConfigureFonts(fonts => {
      fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
      fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    });

#if DEBUG
    builder.Logging.AddDebug();
#endif

    return builder;
  }
}