using GC.Common;
using GC.Frontend.ViewModels;
using GC.Models;
using System.ComponentModel;

namespace GC.iOS.Controllers;

/// <summary>
/// View controller for displaying billing information and transactions.
/// Shows monthly totals and transaction history with swipe navigation.
/// </summary>
public class BillingViewController : BaseViewController, IUITableViewDataSource, IUITableViewDelegate
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
    /// Activity indicator shown when data is loading.
    /// </summary>
    private UIActivityIndicatorView? _loadingIndicator;

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

        // Create loading indicator
        _loadingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Medium) {
            Center = new CGPoint(View.Bounds.Width / 2, safeArea.Top + 38),
            HidesWhenStopped = true
        };
        _topView.AddSubview(_loadingIndicator);

        // Create transactions table view
        _transactionsTable = new UITableView(new CGRect(0, safeArea.Top + 76, View.Bounds.Width, View.Bounds.Height - safeArea.Top - 76 - safeArea.Bottom), UITableViewStyle.Plain) {
            DataSource = this,
            Delegate = this,
            RowHeight = 80 // Make rows bigger for the blocks
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
        _loadingIndicator!.Center = new CGPoint(View.Bounds.Width / 2, safeArea.Top + 38);
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
            _selectedTotalLabel!.Text = "";
            _lastMonthLabel!.Text = "";
        }

        // Reload table data and update page control
        _transactionsTable!.ReloadData();
        _pageControl!.Pages = _viewModel.AvailableMonths.Count;
        _pageControl!.CurrentPage = _viewModel.SelectedIndex;

        // Show or hide loading indicator
        if (_viewModel.IsLoading)
        {
            _loadingIndicator!.StartAnimating();
            _transactionsTable!.Hidden = true;
        }
        else
        {
            _loadingIndicator!.StopAnimating();
            _transactionsTable!.Hidden = false;
        }
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
            return 2; // One row for Gourmet, one for Cafe&Co
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
        cell.TextLabel.Font = UIFont.SystemFontOfSize(20); // Larger font for the blocks
        cell.TextLabel.Lines = 2; // Allow multiple lines
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            var selectedMonth = _viewModel.AvailableMonths[_viewModel.SelectedIndex];
            if (indexPath.Row == 0)
            {
                cell.TextLabel.Text = $"Gourmet\n{selectedMonth.CountGourmet} Zahlungen in Summe {selectedMonth.TotalGourmet:N2} €";
            }
            else if (indexPath.Row == 1)
            {
                cell.TextLabel.Text = $"Cafe&Co\n{selectedMonth.CountCafeAndCo} Zahlungen in Summe {selectedMonth.TotalCafeAndCo:N2} €";
            }
        }
        return cell;
    }

    /// <summary>
    /// Called when a row is selected.
    /// </summary>
    /// <param name="tableView">The table view.</param>
    /// <param name="indexPath">The index path of the selected row.</param>
    public void RowSelected(UITableView tableView, NSIndexPath indexPath)
    {
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            var selectedMonth = _viewModel.AvailableMonths[_viewModel.SelectedIndex];
            Transaction[] transactions;
            string title;
            if (indexPath.Row == 0)
            {
                transactions = selectedMonth.Transactions.Where(t => t.Type == Transaction.TransactionType.Gourmet).ToArray();
                title = "Gourmet";
            }
            else if (indexPath.Row == 1)
            {
                transactions = selectedMonth.Transactions.Where(t => t.Type == Transaction.TransactionType.CafePlusCo).ToArray();
                title = "Cafe&Co";
            }
            else
            {
                return;
            }

            var listController = new TransactionListViewController(transactions);
            listController.Title = title;
            NavigationController?.PushViewController(listController, true);
        }
    }
}
