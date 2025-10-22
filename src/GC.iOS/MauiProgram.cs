using GC.Frontend.Mobile;
using Microsoft.Maui.Hosting;

namespace GC.iOS;

public static class MauiProgram {
  public static MauiApp CreateMauiApp() {
    var builder = MauiApp.CreateBuilder();

    builder
      .UseSharedMauiApp();

    return builder.Build();
  }
}