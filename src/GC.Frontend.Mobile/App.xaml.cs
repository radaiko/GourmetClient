using System;
using GC.Frontend.Mobile.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace GC.Frontend.Mobile;

public partial class App {
  public App() {
    InitializeComponent();

    var platform = DeviceInfo.Current.Platform;
    Resources.MergedDictionaries.Clear();

    if (platform == DevicePlatform.iOS)
      Resources.MergedDictionaries.Add("Resources/Styles/Apples.xaml");
    else if (platform == DevicePlatform.Android)
      Resources.MergedDictionaries.Add("Resources/Styles/Android.xaml");

    // Do not set MainPage here — initialize the app window by overriding CreateWindow instead.
    // MainPage is obsolete for initialization in .NET MAUI; CreateWindow will return a Window containing the root page.
  }

  protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
