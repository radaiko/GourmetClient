using System;
using GC.Frontend.Mobile.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace GC.Frontend.Mobile;

public partial class App {
  public App() {
    InitializeComponent();
    // Merge platform-specific resource dictionaries at runtime to avoid XAML parsing issues
    if (DeviceInfo.Platform == DevicePlatform.iOS)
    {
      MergePlatformResources("Resources/Styles/Colors.ios.xaml", "Resources/Styles/Styles.ios.xaml");
    }
    else if (DeviceInfo.Platform == DevicePlatform.Android)
    {
      MergePlatformResources("Resources/Styles/Colors.android.xaml", "Resources/Styles/Styles.android.xaml");
    }

    // Do not set MainPage here — initialize the app window by overriding CreateWindow instead.
    // MainPage is obsolete for initialization in .NET MAUI; CreateWindow will return a Window containing the root page.
  }

  void MergePlatformResources(string colorsSource, string stylesSource)
  {
    try
    {
      var rd = this.Resources ??= new ResourceDictionary();

      // Add platform color dictionary first so styles can reference color keys
      rd.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(colorsSource, UriKind.Relative) });
      rd.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(stylesSource, UriKind.Relative) });
    }
    catch (Exception)
    {
      // ignore errors here; fallback to defaults is acceptable
    }
  }

  protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}