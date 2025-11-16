using System.Collections.Specialized;
using Foundation;
using GC.Common;
using GC.Frontend.ViewModels;
using UIKit;

namespace GC.iOS.Controllers;

/// <summary>
/// View controller for displaying application logs in debug mode.
/// Shows log messages with their level, timestamp, and details.
/// </summary>
public class LogViewController : BaseViewController, IUITableViewDataSource {
  /// <summary>
  /// View model that provides access to log data.
  /// </summary>
  private LogViewModel _viewModel = new();

  /// <summary>
  /// Table view for displaying log messages.
  /// </summary>
  private UITableView? _table;

  /// <summary>
  /// Reuse identifier for log table cells.
  /// </summary>
  private const string CellIdentifier = "LogCell";

  /// <summary>
  /// Called after the view has been loaded into memory.
  /// Sets up the table view and subscribes to log updates.
  /// </summary>
  public override void ViewDidLoad() {
    base.ViewDidLoad();
    // Create the main table view
    _table = new UITableView(View.Bounds, UITableViewStyle.Plain) {
      AutoresizingMask = UIViewAutoresizing.All,
      BackgroundColor = UIColor.SystemBackground,
      RowHeight = UITableView.AutomaticDimension,
      EstimatedRowHeight = 80
    };
    _table.DataSource = this;
    _table.Delegate = new LogTableDelegate(this);

    View.AddSubview(_table);

    // Subscribe to collection changes to automatically refresh the table
    _viewModel.Logs.CollectionChanged += OnLogsChanged;

    // Add a clear button to the navigation bar
    var clearButton = new UIBarButtonItem(UIBarButtonSystemItem.Trash, (sender, e) => {
      _viewModel.Logs.Clear();
      _table.ReloadData();
    });
    NavigationItem.RightBarButtonItem = clearButton;

    // Initial load
    _table.ReloadData();
  }

  /// <summary>
  /// Called when logs collection changes to update the table view.
  /// </summary>
  private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
    InvokeOnMainThread(() => {
      _table?.ReloadData();
      
      // Auto-scroll to the bottom to show the newest log
      if (_viewModel.Logs.Count > 0) {
        var lastIndexPath = NSIndexPath.FromRowSection(_viewModel.Logs.Count - 1, 0);
        _table?.ScrollToRow(lastIndexPath, UITableViewScrollPosition.Bottom, true);
      }
    });
  }

  /// <summary>
  /// Returns the number of rows in the table view.
  /// </summary>
  public nint RowsInSection(UITableView tableView, nint section) {
    return _viewModel.Logs.Count;
  }

  /// <summary>
  /// Creates and configures a cell for the given index path.
  /// </summary>
  public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
    var cell = tableView.DequeueReusableCell(CellIdentifier) ?? 
               new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier);
    
    if (indexPath.Row < _viewModel.Logs.Count) {
      var log = _viewModel.Logs[indexPath.Row];
      
      // Configure the cell with log information
      if (cell.TextLabel != null) {
        cell.TextLabel.Text = $"[{log.Level}] {log.Message}";
        cell.TextLabel.Lines = 2; // Limit to 2 lines
        cell.TextLabel.LineBreakMode = UILineBreakMode.TailTruncation;
        cell.TextLabel.Font = UIFont.SystemFontOfSize(12);
        
        // Set color based on log level
        cell.TextLabel.TextColor = log.Level switch {
          LogLevel.Error => UIColor.SystemRed,
          LogLevel.Warning => UIColor.SystemOrange,
          LogLevel.Debug => UIColor.SystemBlue,
          _ => UIColor.Label
        };
      }
      
      if (cell.DetailTextLabel != null) {
        cell.DetailTextLabel.Text = $"{log.Timestamp} | {log.Class}.{log.Method}";
        cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(10);
        cell.DetailTextLabel.TextColor = UIColor.SecondaryLabel;
      }
      
      // Allow selection to show full log details
      cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
    }
    
    return cell;
  }

  /// <summary>
  /// Shows full log details in an alert dialog.
  /// </summary>
  private void ShowLogDetails(LogMsg log) {
    var alert = UIAlertController.Create(
      $"[{log.Level}] Log Details",
      $"Time: {log.Timestamp}\nClass: {log.Class}\nMethod: {log.Method}\n\nMessage:\n{log.Message}",
      UIAlertControllerStyle.Alert
    );
    
    alert.AddAction(UIAlertAction.Create("Copy", UIAlertActionStyle.Default, _ => {
      UIPasteboard.General.String = log.ToString();
    }));
    
    alert.AddAction(UIAlertAction.Create("Close", UIAlertActionStyle.Cancel, null));
    
    PresentViewController(alert, true, null);
  }

  /// <summary>
  /// Disposes of resources used by the view controller.
  /// </summary>
  protected override void Dispose(bool disposing) {
    if (disposing) {
      _viewModel.Logs.CollectionChanged -= OnLogsChanged;
    }
    base.Dispose(disposing);
  }

  /// <summary>
  /// Table view delegate for handling cell selection.
  /// </summary>
  private class LogTableDelegate : UITableViewDelegate {
    private readonly LogViewController _controller;

    public LogTableDelegate(LogViewController controller) {
      _controller = controller;
    }

    public override void RowSelected(UITableView tableView, NSIndexPath indexPath) {
      // Deselect the row for visual feedback
      tableView.DeselectRow(indexPath, true);
      
      // Show full log details
      if (indexPath.Row < _controller._viewModel.Logs.Count) {
        var log = _controller._viewModel.Logs[indexPath.Row];
        _controller.ShowLogDetails(log);
      }
    }
  }
}