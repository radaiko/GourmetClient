using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace GourmetClient.MVU;

#if !IOS
internal class Program {
  public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .LogToTrace();
}
#endif

public class App : Application {
  public override void Initialize() {
    Styles.Add(new FluentTheme());
    Name = "Gourmet Client";
  }

  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      // Desktop application uses a window host
      desktop.MainWindow = new MainWindow();
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
      // Mobile application (iOS) uses a control host
      singleViewPlatform.MainView = new MainViewHostControl();
    }

    base.OnFrameworkInitializationCompleted();
  }
}
