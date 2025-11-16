using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using GC.Common;

namespace GC.Frontend.ViewModels;

public class LogViewModel : ObservableObject {
  /// <summary>
  /// Gets the observable collection of log messages from the Logger.
  /// </summary>
  public ObservableCollection<LogMsg> Logs => Logger.Logs;
}