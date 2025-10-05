using Avalonia.Controls;
using GC.ViewModels;
using GC.ViewModels.Services;
using GourmetClient.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GC.Views;

public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    
    // Get services from DI container
    var settingsService = ServiceProviderHolder.Services.GetRequiredService<GourmetSettingsService>();
    var logger = ServiceProviderHolder.Services.GetRequiredService<ILogger<MainViewModel>>();
    
    DataContext = new MainViewModel(settingsService, logger);
  }
}

