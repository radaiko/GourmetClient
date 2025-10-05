using Avalonia.Controls;
using GC.ViewModels;
using GourmetClient.Core.Settings;
using GourmetClient.Core.Utils;

namespace GC.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    
    // Create GourmetSettingsService with FilePathProvider
    var settingsService = new GourmetSettingsService(new FilePathProvider());
    DataContext = new MainViewModel(settingsService);
  }
}

