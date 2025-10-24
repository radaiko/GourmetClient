using GC.Frontend.ViewModels;

namespace GC.iOS.Controllers;

public class BillingViewController : UIViewController, IUITableViewDataSource
{
    private BillingViewModel _viewModel = new();
    private UILabel? _selectedTotalLabel;
    private UILabel? _lastMonthLabel;
    private UITableView? _transactionsTable;
    private UIPageControl? _pageControl;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View.BackgroundColor = UIColor.SystemBackground;

        var safeArea = Common.SafeArea;
        
        // Create views with safe area frames
        var topView = new UIView(new CGRect(0, safeArea.Top, View.Bounds.Width, 76));
        _pageControl = new UIPageControl(new CGRect(0, 0, View.Bounds.Width, 16));
        _pageControl.Pages = _viewModel.AvailableMonths.Count;
        _pageControl.CurrentPage = _viewModel.SelectedIndex;
        _pageControl.ValueChanged += (sender, e) => {
            _viewModel.SelectedIndex = (int)_pageControl.CurrentPage;
            UpdateUI();
        };
        topView.AddSubview(_pageControl);
        _selectedTotalLabel = new UILabel(new CGRect(16, 16, View.Bounds.Width / 2 - 16, 44)) {
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(18),
            Lines = 2
        };
        _lastMonthLabel = new UILabel(new CGRect(View.Bounds.Width / 2, 16, View.Bounds.Width / 2 - 16, 44)) {
            TextAlignment = UITextAlignment.Right,
            Font = UIFont.SystemFontOfSize(18),
            Lines = 2
        };
        topView.AddSubview(_selectedTotalLabel);
        topView.AddSubview(_lastMonthLabel);
        View.AddSubview(topView);

        // Transactions table
        _transactionsTable = new UITableView(new CGRect(0, safeArea.Top + 76, View.Bounds.Width, View.Bounds.Height - safeArea.Top - 76), UITableViewStyle.Plain) {
            DataSource = this
        };
        var refreshControl = new UIRefreshControl();
        refreshControl.ValueChanged += async (sender, e) => {
            await _viewModel.RefreshBillingDataCommand.ExecuteAsync(null);
            refreshControl.EndRefreshing();
        };
        _transactionsTable.RefreshControl = refreshControl;
        View.AddSubview(_transactionsTable);

        // Swipe gestures
        var leftSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Left };
        leftSwipe.AddTarget(() => {
            if (_viewModel.SelectedIndex < _viewModel.AvailableMonths.Count - 1) {
                _viewModel.SelectedIndex++;
            }
        });
        View.AddGestureRecognizer(leftSwipe);

        var rightSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Right };
        rightSwipe.AddTarget(() => {
            if (_viewModel.SelectedIndex > 0) {
                _viewModel.SelectedIndex--;
            }
        });
        View.AddGestureRecognizer(rightSwipe);

        // Bind to ViewModel
        _viewModel.PropertyChanged += (_, _) => UpdateUI();

        UpdateUI();
    }

    private void UpdateUI()
    {
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

        _transactionsTable!.ReloadData();
        _pageControl!.Pages = _viewModel.AvailableMonths.Count;
        _pageControl!.CurrentPage = _viewModel.SelectedIndex;
    }

    public nint NumberOfSections(UITableView tableView) => 1;

    public nint RowsInSection(UITableView tableView, nint section)
    {
        if (_viewModel.AvailableMonths.Count > _viewModel.SelectedIndex)
        {
            return _viewModel.AvailableMonths[_viewModel.SelectedIndex].Transactions.Length;
        }
        return 0;
    }

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
