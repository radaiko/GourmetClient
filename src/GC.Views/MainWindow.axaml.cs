using Avalonia.Controls;
using GC.ViewModels;

namespace GC.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    DataContext = new MainViewModel();
  }
}

