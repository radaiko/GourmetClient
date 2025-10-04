#if __IOS__
using Avalonia.Controls.ApplicationLifetimes;

namespace GourmetClient.MVU;

public partial class App {
  partial void HookSingleViewLifetime(ISingleViewApplicationLifetime lifetime) {
    lifetime.MainView = new MainViewHostControl();
  }
}
#endif
