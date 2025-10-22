#region
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
#endregion

namespace GC.Frontend.Desktop.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
  }
  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}