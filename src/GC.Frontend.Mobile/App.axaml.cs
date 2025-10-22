#region
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GC.Frontend.Mobile.Views;
using GC.Frontend.ViewModels;
#endregion

namespace GC.Frontend.Mobile;

public class App : Application {
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
      singleViewPlatform.MainView = new MainView {
        DataContext = new MainViewModel()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}