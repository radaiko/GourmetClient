using System;
using Avalonia.Controls;
using GC.ViewModels;

namespace GC.Views;

public partial class MainView : UserControl {
  public MainView() {
    if (!OperatingSystem.IsIOS()) {
      InitializeComponent();
    }
  }

  protected override void OnDataContextChanged(EventArgs e) {
    base.OnDataContextChanged(e);
    if (OperatingSystem.IsIOS() && DataContext is MainViewModel viewModel) {
      Content = MainViewMobile.Create(viewModel);
    }
  }
}