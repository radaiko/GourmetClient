using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GC.Views.Main;

public partial class MainViewMobile : UserControl
{
    public MainViewMobile()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
