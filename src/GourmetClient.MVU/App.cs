using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace GourmetClient.MVU;

internal class Program {
  public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .LogToTrace();
}

public class App : Application {
  public override void Initialize() {
    Styles.Add(new FluentTheme());
    Name = "Gourmet Client";
  }

  public override void OnFrameworkInitializationCompleted() {
    var mainWindow = new MainWindow();

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      desktop.MainWindow = mainWindow;
    }

    base.OnFrameworkInitializationCompleted();
  }
}