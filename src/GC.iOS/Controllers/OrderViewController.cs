using UIKit;
using GC.Frontend.ViewModels;
using System.Linq;
using GC.Common;
using GC.iOS.Core;
using System;
using System.ComponentModel;

namespace GC.iOS.Controllers;

public class OrderViewController : UIViewController, IUITableViewDataSource
{
    private OrderViewModel _viewModel = new();
    private UILabel? _dateLabel;
    private UITableView? _menusTable;
    private UIPageControl? _pageControl;
    private SafeAreaHelper<OrderViewController> _safeAreaHelper;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        
        _safeAreaHelper = new SafeAreaHelper<OrderViewController>(this);
        _safeAreaHelper.PropertyChanged += OnSafeAreaChanged;
        
        CreateUI();
        OnSafeAreaChanged(null, new PropertyChangedEventArgs(nameof(SafeAreaHelper<OrderViewController>.SafeAreaInsets)));
        
        // Bind to ViewModel
        _viewModel.PropertyChanged += (_, _) => UpdateUI();
        
        UpdateUI();
    }

    private void CreateUI() {
        var safeArea = _safeAreaHelper.SafeAreaInsets;
        Log.Debug($"Safe area - Top: {safeArea.Top}, Bottom: {safeArea.Bottom}, Left: {safeArea.Left}, Right: {safeArea.Right}");
        var keyWindow = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(s => s.Windows)
            .LastOrDefault(w => w.IsKeyWindow); 
        Log.Debug($"UIApplication keyWindow = {keyWindow}");
        // Page control on top
        _pageControl = new UIPageControl(new CGRect(0, safeArea.Top, View.Bounds.Width, 40));
        _pageControl.Pages = _viewModel.AvailableDays.Count;
        _pageControl.CurrentPage = _viewModel.SelectedIndex;
        _pageControl.ValueChanged += (sender, e) => {
            _viewModel.SelectedIndex = (int)_pageControl.CurrentPage;
            UpdateUI();
        };
        View.AddSubview(_pageControl);

        // Date label below
        _dateLabel = new UILabel(new CGRect(16, safeArea.Top + 40, View.Bounds.Width - 32, 40)) {
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(20),
            Lines = 1
        };
        View.AddSubview(_dateLabel);

        // Menus table
        var tableHeight = View.Bounds.Height - safeArea.Top - 80 - safeArea.Bottom;
        _menusTable = new UITableView(new CGRect(0, safeArea.Top + 80, View.Bounds.Width, tableHeight), UITableViewStyle.Plain) {
            DataSource = this
        };
        _menusTable.RowHeight = tableHeight / 4;
        View.AddSubview(_menusTable);

        // Swipe gestures
        var leftSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Left };
        leftSwipe.AddTarget(() => {
            Log.Debug($"Left swipe detected, current index: {_viewModel.SelectedIndex}");
            if (_viewModel.SelectedIndex < _viewModel.AvailableDays.Count - 1) {
                _viewModel.SelectedIndex++;
                Log.Debug($"Changed index to {_viewModel.SelectedIndex}");
            }
        });
        _menusTable.AddGestureRecognizer(leftSwipe);

        var rightSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Right };
        rightSwipe.AddTarget(() => {
            Log.Debug($"Right swipe detected, current index: {_viewModel.SelectedIndex}");
            if (_viewModel.SelectedIndex > 0) {
                _viewModel.SelectedIndex--;
                Log.Debug($"Changed index to {_viewModel.SelectedIndex}");
            }
        });
        _menusTable.AddGestureRecognizer(rightSwipe);
    }

    private void OnSafeAreaChanged(object? sender, PropertyChangedEventArgs e)
    {
        var safeArea = _safeAreaHelper.SafeAreaInsets;
        Log.Debug($"OnSafeAreaChanged called, insets: Top={safeArea.Top}, Bottom={safeArea.Bottom}, Left={safeArea.Left}, Right={safeArea.Right}");
        _pageControl!.Frame = new CGRect(0, safeArea.Top, View.Bounds.Width, 40);
        _dateLabel!.Frame = new CGRect(16, safeArea.Top + 40, View.Bounds.Width - 32, 40);
        var tableY = safeArea.Top + 80;
        var tableHeight = View.Bounds.Height - safeArea.Top - 80 - safeArea.Bottom;
        _menusTable!.Frame = new CGRect(0, tableY, View.Bounds.Width, tableHeight);
        _menusTable!.RowHeight = tableHeight / 4;
    }

    private void UpdateUI()
    {
        Log.Debug($"UpdateUI called, selected index: {_viewModel.SelectedIndex}, available days: {_viewModel.AvailableDays.Count}");
        if (_viewModel.AvailableDays.Count > _viewModel.SelectedIndex)
        {
            var selectedDay = _viewModel.AvailableDays[_viewModel.SelectedIndex];
            _dateLabel!.Text = selectedDay.Date.ToString("dddd, MMMM d, yyyy");
        }
        else
        {
            _dateLabel!.Text = "-";
        }

        _menusTable!.ReloadData();
        _pageControl!.Pages = _viewModel.AvailableDays.Count;
        _pageControl!.CurrentPage = _viewModel.SelectedIndex;
        
        var keyWindow = UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .LastOrDefault(w => w.IsKeyWindow); 
        Log.Debug($"UIApplication keyWindow = {keyWindow}");
    }

    public nint NumberOfSections(UITableView tableView) => 1;

    public nint RowsInSection(UITableView tableView, nint section)
    {
        if (_viewModel.AvailableDays.Count > _viewModel.SelectedIndex)
        {
            return _viewModel.AvailableDays[_viewModel.SelectedIndex].Menus.Count;
        }
        return 0;
    }

    public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
    {
        var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);
        if (_viewModel.AvailableDays.Count > _viewModel.SelectedIndex)
        {
            var menu = _viewModel.AvailableDays[_viewModel.SelectedIndex].Menus[indexPath.Row];
            cell.TextLabel.Text = $"{menu.Title}";
            cell.TextLabel.Lines = 0;
            cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            cell.DetailTextLabel.Text = $"{menu.Price:N2} € - Allergens: {string.Join(", ", menu.Allergens)}";
        }
        return cell;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _safeAreaHelper.PropertyChanged -= OnSafeAreaChanged;
            _safeAreaHelper.Dispose();
        }
        base.Dispose(disposing);
    }
}
