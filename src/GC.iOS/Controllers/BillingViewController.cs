using GC.Common;
using GC.Frontend.ViewModels;
using System.ComponentModel;

namespace GC.iOS.Controllers;

/// <summary>
/// View controller for displaying billing information and transactions.
/// Shows monthly totals and transaction history with swipe navigation.
/// </summary>
public class BillingViewController : BaseViewController, IUITableViewDataSource
{
    /// <summary>
    /// View model that provides billing data.
    /// </summary>
    private BillingViewModel _viewModel = new();

    /// <summary>
    /// Label showing the selected month and year.
    /// </summary>
    private UILabel? _selectedTotalLabel;

    /// <summary>
    /// Label showing the total amount for the selected month.
    /// </summary>
    private UILabel? _lastMonthLabel;

    /// <summary>
    /// Table view displaying the transactions for the selected month.
    /// </summary>
    private UITableView? _transactionsTable;

    /// <summary>
    /// Page control for navigating between different months.
    /// </summary>
    private UIPageControl? _pageControl;

    /// <summary>
    /// Top view containing the page control and labels.
    /// </summary>
    private UIView? _topView;

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Sets up the UI elements, binds to the view model, and initializes the display.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Set background color
        View.BackgroundColor = UIColor.SystemBackground;

        var safeArea = _safeAreaHelper.SafeAreaInsets;
        Log.Debug($"Safe area - Top: {safeArea.Top}, Bottom: {safeArea.Bottom}, Left: {safeArea.Left}, Right: {safeArea.Right}");

        // Create top view for month info and page control
        _topView = new UIView(new CGRect(0, safeArea.Top, View.Bounds.Width, 76));

        // Create page control for month navigation
        _pageControl = new UIPageControl(new CGRect(0, 0, View.Bounds.Width, 16));
        _pageControl.Pages = _viewModel.AvailableMonths.Count;
        _pageControl.CurrentPage = _viewModel.SelectedIndex;
        _pageControl.ValueChanged += (sender, e) => {
            _viewModel.SelectedIndex = (int)_pageControl.CurrentPage;
            UpdateUI();
        };
        _topView.AddSubview(_pageControl);

        // Create label for selected month
        _selectedTotalLabel = new UILabel(new CGRect(16, 16, View.Bounds.Width / 2 - 16, 44)) {
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(18),
            Lines = 2
        };

        // Create label for total amount
        _lastMonthLabel = new UILabel(new CGRect(View.Bounds.Width / 2, 16, View.Bounds.Width / 2 - 16, 44)) {
            TextAlignment = UITextAlignment.Right,
            Font = UIFont.SystemFontOfSize(18),
            Lines = 2
        };
        _topView.AddSubview(_selectedTotalLabel);
        _topView.AddSubview(_lastMonthLabel);
        View.AddSubview(_topView);

        // Create transactions table view
        _transactionsTable = new UITableView(new CGRect(0, safeArea.Top + 76, View.Bounds.Width, View.Bounds.Height - safeArea.Top - 76 - safeArea.Bottom), UITableViewStyle.Plain) {
            DataSource = this
        };

        // Add pull-to-refresh functionality
        var refreshControl = new UIRefreshControl();
        refreshControl.ValueChanged += async (sender, e) => {
            await _viewModel.RefreshCommand.ExecuteAsync(null);
            refreshControl.EndRefreshing();
        };
        _transactionsTable.RefreshControl = refreshControl;
        View.AddSubview(_transactionsTable);

        // Add swipe gesture for next month
        var leftSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Left };
        leftSwipe.AddTarget(() => {
            if (_viewModel.SelectedIndex < _viewModel.AvailableMonths.Count - 1) {
                _viewModel.SelectedIndex++;
            }
        });
        _transactionsTable.AddGestureRecognizer(leftSwipe);

        // Add swipe gesture for previous month
        var rightSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Right };
        rightSwipe.AddTarget(() => {
            if (_viewModel.SelectedIndex > 0) {
                _viewModel.SelectedIndex--;
            }
        });
        _transactionsTable.AddGestureRecognizer(rightSwipe);

        // Bind to view model changes
        _viewModel.AvailableMonths.CollectionChanged  += (_, _) => UpdateUI();
        _viewModel.PropertyChanged += (_, _) => UpdateUI();

        // Initial UI update
        UpdateUI();

        // Initial safe area adjustment
        OnSafeAreaChanged(this, new PropertyChangedEventArgs(string.Empty));
    }

    /// <summary>
    /// Called when the safe area insets change.
    /// Adjusts the layout of UI elements to fit the new safe area.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Event arguments.</param>
    protected override void OnSafeAreaChanged(object? sender, PropertyChangedEventArgs e)
    {
        var safeArea = _safeAreaHelper.SafeAreaInsets;
        _topView!.Frame = new CGRect(0, safeArea.Top, View.Bounds.Width, 76);
        _transactionsTable!.Frame = new CGRect(0, safeArea.Top + 76, View.Bounds.Width, View.Bounds.Height - safeArea.Top - 76 - safeArea.Bottom);
    }

    /// <summary>
    /// Updates the UI to reflect the current state of the view model.
    /// </summary>
    private void UpdateUI()
    {
        // Update labels with selected month info
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            var selectedMonth = _viewModel.AvailableMonths[_viewModel.SelectedIndex];
            _selectedTotalLabel!.Text = selectedMonth.Month.ToString("MMMM yyyy");
            _lastMonthLabel!.Text = $"{selectedMonth.TotalAmount:N2} €";
        }
        else
        {
            _selectedTotalLabel!.Text = "-";
            _lastMonthLabel!.Text = "-";
        }

        // Reload table data and update page control
        _transactionsTable!.ReloadData();
        _pageControl!.Pages = _viewModel.AvailableMonths.Count;
        _pageControl!.CurrentPage = _viewModel.SelectedIndex;
    }

    /// <summary>
    /// Returns the number of sections in the table view.
    /// </summary>
    /// <param name="tableView">The table view requesting the information.</param>
    /// <returns>The number of sections (always 1).</returns>
    public nint NumberOfSections(UITableView tableView) => 1;

    /// <summary>
    /// Returns the number of rows in the specified section.
    /// </summary>
    /// <param name="tableView">The table view requesting the information.</param>
    /// <param name="section">The section index.</param>
    /// <returns>The number of rows in the section.</returns>
    public nint RowsInSection(UITableView tableView, nint section)
    {
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            return _viewModel.AvailableMonths[_viewModel.SelectedIndex].Transactions.Length;
        }
        return 0;
    }

    /// <summary>
    /// Returns a cell for the specified index path.
    /// </summary>
    /// <param name="tableView">The table view requesting the cell.</param>
    /// <param name="indexPath">The index path of the cell.</param>
    /// <returns>The configured table view cell.</returns>
    public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
    {
        var cell = new UITableViewCell(UITableViewCellStyle.Default, null);
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            var transaction = _viewModel.AvailableMonths[_viewModel.SelectedIndex].Transactions[indexPath.Row];
            var total = transaction.Positions.Sum(p => p.TotalPrice);
            cell.TextLabel.Text = $"{transaction.Date:dd.MM.yyyy} - {transaction.Type} - {total:N2} €";
        }
        return cell;
    }
}
