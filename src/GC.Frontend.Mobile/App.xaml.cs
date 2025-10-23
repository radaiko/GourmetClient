using System.Linq;
using GC.Frontend.Mobile.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace GC.Frontend.Mobile;

public partial class App {
  public App() {
    InitializeComponent();

    var platform = DeviceInfo.Current.Platform;

    foreach (var dict in Resources.MergedDictionaries.ToList()) {
      if (dict.Source.OriginalString.Contains("Android") && platform == DevicePlatform.iOS)
        Resources.MergedDictionaries.Remove(dict);
      else if (dict.Source.OriginalString.Contains("Apple") && platform == DevicePlatform.Android)
        Resources.MergedDictionaries.Remove(dict);
    }

    // Do not set MainPage here — initialize the app window by overriding CreateWindow instead.
    // MainPage is obsolete for initialization in .NET MAUI; CreateWindow will return a Window containing the root page.
  }

  protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
