// Clarification: `MainView` is a UserControl (re-usable piece of UI) that contains the
// content shown inside top-level Windows. Windows (like `MainWindowDesktop` or `MainWindow`)
// host UserControls. In this project `MainWindowDesktop` hosts `MainView` for the desktop UI.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GC.Views.Main;

public partial class MainView : UserControl
{
    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
