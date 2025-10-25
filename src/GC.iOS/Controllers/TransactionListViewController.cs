using System.ComponentModel;
using GC.Models;

namespace GC.iOS.Controllers;

/// <summary>
///   View controller for displaying a list of transactions.
/// </summary>
public class TransactionListViewController : BaseViewController, IUITableViewDataSource {
  private readonly Transaction[] _transactions;
  private List<(DateTime Date, List<Position> Positions)> _groupedData;

  private UITableView? _tableView;

  public TransactionListViewController(Transaction[] transactions) => _transactions = transactions;

  public override void ViewDidLoad() {
    base.ViewDidLoad();

    // Group transactions by date
    _groupedData = _transactions
      .GroupBy(t => t.Date.Date)
      .Select(g => (Date: g.Key, Positions: g.SelectMany(t => t.Positions).ToList()))
      .OrderBy(g => g.Date)
      .ToList();

    View.BackgroundColor = UIColor.SystemBackground;

    var safeArea = _safeAreaHelper.SafeAreaInsets;

    // Create table view
    _tableView = new UITableView(new CGRect(0, safeArea.Top, View.Bounds.Width, View.Bounds.Height - safeArea.Top - safeArea.Bottom), UITableViewStyle.Plain) {
      DataSource = this,
      RowHeight = 70 // Adjusted row height to remove empty space
    };

    View.AddSubview(_tableView);

    // Initial safe area adjustment
    OnSafeAreaChanged(this, new PropertyChangedEventArgs(string.Empty));
  }

  protected override void OnSafeAreaChanged(object? sender, PropertyChangedEventArgs e) {
    var safeArea = _safeAreaHelper.SafeAreaInsets;
    _tableView!.Frame = new CGRect(0, safeArea.Top, View.Bounds.Width, View.Bounds.Height - safeArea.Top - safeArea.Bottom);
  }

  public nint NumberOfSections(UITableView tableView) => _groupedData.Count;

  public nint RowsInSection(UITableView tableView, nint section) => _groupedData[(int)section].Positions.Count;

  public string? TitleForHeader(UITableView tableView, nint section) {
    var group = _groupedData[(int)section];
    var total = group.Positions.Sum(p => p.TotalPrice);
    return $"{group.Date:dd.MM.yyyy} - Total: {total:N2} €";
  }

  public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
    var cell = new UITableViewCell(UITableViewCellStyle.Default, null);
    cell.TextLabel.Text = ""; // Hide the default label
    var group = _groupedData[(int)indexPath.Section];
    var position = group.Positions[indexPath.Row];

    // Title label: 1x Name
    var titleLabel = new UILabel {
      Text = $"{position.Quantity}x {position.Name}",
      Font = UIFont.BoldSystemFontOfSize(16),
      TextColor = UIColor.Label
    };
    titleLabel.Frame = new CGRect(16, 8, cell.ContentView.Bounds.Width - 32, 20);
    cell.ContentView.AddSubview(titleLabel);

    // Support label on the left
    var supportLabel = new UILabel {
      Text = $"Stützung: {position.Support:N2} €",
      Font = UIFont.SystemFontOfSize(14),
      TextColor = UIColor.SecondaryLabel,
      TextAlignment = UITextAlignment.Left
    };
    supportLabel.Frame = new CGRect(16, 36, (cell.ContentView.Bounds.Width - 32) / 2, 20);
    cell.ContentView.AddSubview(supportLabel);

    // Total price on the right
    var totalLabel = new UILabel {
      Text = $"{position.TotalPrice:N2} €",
      Font = UIFont.SystemFontOfSize(14),
      TextColor = UIColor.Label,
      TextAlignment = UITextAlignment.Right
    };
    totalLabel.Frame = new CGRect(16 + (cell.ContentView.Bounds.Width - 32) / 2, 36, (cell.ContentView.Bounds.Width - 32) / 2, 20);
    cell.ContentView.AddSubview(totalLabel);

    return cell;
  }
}