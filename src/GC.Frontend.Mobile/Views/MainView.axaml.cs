#region
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
#endregion

namespace GC.Frontend.Mobile.Views;

public partial class MainView : UserControl {
  public MainView() {
    InitializeComponent();
  }
  
  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}