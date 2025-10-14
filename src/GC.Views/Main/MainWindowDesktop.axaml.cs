// Clarification: "Window" types are top-level platform windows (they host content and are shown by the
// application lifetime). This Desktop-specific window (`MainWindowDesktop`) is the app's desktop entry point
// and hosts the shared `MainView` UserControl for the desktop UI.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GC.Views.Main;

public partial class MainWindowDesktop : Window
{
    public MainWindowDesktop()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
