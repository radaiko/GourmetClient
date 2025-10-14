// Clarification: This `MainWindow` is a top-level Window type (like the Desktop window),
// historically present in the project. A `Window` is shown by the application lifetime
// and hosts content (usually UserControls). We created a separate `MainWindowDesktop`
// to make the desktop-specific entry point explicit; `MainWindow` remains here for
// compatibility or other platform-specific uses.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GC.Views.Main;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
