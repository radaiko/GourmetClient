using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace GourmetClient.MVU;

public partial class App : Application {
  public override void Initialize() {
    Styles.Add(new FluentTheme());
    Name = "Gourmet Client";
  }

  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      // Desktop main window host
      desktop.MainWindow = new MainWindow();
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime) {
      // Delegate iOS SingleView setup to platform-specific partial implementation
      HookSingleViewLifetime(lifetime);
    }

    base.OnFrameworkInitializationCompleted();
  }

  // Desktop build provides no-op; iOS partial supplies implementation.
  partial void HookSingleViewLifetime(ISingleViewApplicationLifetime lifetime);
}
