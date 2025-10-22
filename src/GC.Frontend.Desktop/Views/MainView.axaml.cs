#region
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
#endregion

namespace GC.Frontend.Desktop.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();
  }
  
  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}