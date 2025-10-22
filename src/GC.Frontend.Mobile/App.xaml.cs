using GC.Frontend.Mobile.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace GC.Frontend.Mobile;

public partial class App : IApplication {
  public App() {
    InitializeComponent();

    // Do not set MainPage here — initialize the app window by overriding CreateWindow instead.
    // MainPage is obsolete for initialization in .NET MAUI; CreateWindow will return a Window containing the root page.
  }

  protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}