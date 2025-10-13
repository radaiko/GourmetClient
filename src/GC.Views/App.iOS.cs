using Avalonia.Controls.ApplicationLifetimes;

namespace GC.Views;

public partial class App {
  partial void HookSingleViewLifetime(ISingleViewApplicationLifetime lifetime) {
    lifetime.MainView = new MainViewHostControl();
  }
}