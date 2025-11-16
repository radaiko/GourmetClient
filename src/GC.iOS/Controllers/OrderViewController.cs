using GC.Frontend.ViewModels;
using GC.Common;
using System.ComponentModel;
using ErrorEventArgs = GC.Common.ErrorEventArgs;

namespace GC.iOS.Controllers;

/// <summary>
/// View controller for displaying and ordering menus.
/// Shows available menus for different days with swipe navigation and page control.
/// </summary>
public class OrderViewController : BaseViewController, IUITableViewDataSource
{
    /// <summary>
    /// View model that provides data for the order view.
    /// </summary>
    private OrderViewModel _viewModel = new();

    /// <summary>
    /// Label displaying the selected date.
    /// </summary>
    private UILabel? _dateLabel;

    /// <summary>
    /// Table view showing the available menus for the selected day.
    /// </summary>
    private UITableView? _menusTable;

    /// <summary>
    /// Page control for navigating between different days.
    /// </summary>
    private UIPageControl? _pageControl;

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Sets up the UI, binds to the view model, and updates the display.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Subscribe to global info/error events so the user is informed about background work
        GC.Common.Base.OnInfo += HandleGlobalInfo;
        GC.Common.Base.OnError += HandleGlobalError;

        // Create the user interface elements
        CreateUI();

        // Update layout for current safe area
        OnSafeAreaChanged(null, new PropertyChangedEventArgs("SafeAreaInsets"));

        // Bind to view model changes to update UI automatically
        _viewModel.PropertyChanged += (_, _) => UpdateUI();

        // Initial UI update
        UpdateUI();
    }

    /// <summary>
    /// Creates and configures all the UI elements for the order view.
    /// </summary>
    private void CreateUI()
    {
        var safeArea = _safeAreaHelper.SafeAreaInsets;
        Logger.Debug($"Safe area - Top: {safeArea.Top}, Bottom: {safeArea.Bottom}, Left: {safeArea.Left}, Right: {safeArea.Right}");

        // Find the key window for debugging purposes
        var keyWindow = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(s => s.Windows)
            .LastOrDefault(w => w.IsKeyWindow);
        Logger.Debug($"UIApplication keyWindow = {keyWindow}");

        // Create page control at the top for day navigation
        _pageControl = new UIPageControl(new CGRect(0, safeArea.Top, View.Bounds.Width, 40));
        _pageControl.Pages = _viewModel.AvailableDays.Count;
        _pageControl.CurrentPage = _viewModel.SelectedIndex;
        _pageControl.ValueChanged += (sender, e) => {
            _viewModel.SelectedIndex = (int)_pageControl.CurrentPage;
            UpdateUI();
        };
        View.AddSubview(_pageControl);

        // Create date label below the page control
        _dateLabel = new UILabel(new CGRect(16, safeArea.Top + 40, View.Bounds.Width - 32, 40)) {
            TextAlignment = UITextAlignment.Left,
            Font = UIFont.SystemFontOfSize(20),
            Lines = 1
        };
        View.AddSubview(_dateLabel);

        // Create table view for displaying menus
        var tableHeight = View.Bounds.Height - safeArea.Top - 80 - safeArea.Bottom;
        _menusTable = new UITableView(new CGRect(0, safeArea.Top + 80, View.Bounds.Width, tableHeight), UITableViewStyle.Plain) {
            DataSource = this
        };
        _menusTable.RowHeight = tableHeight / 4;
        View.AddSubview(_menusTable);

        // Add swipe gesture for navigating to next day
        var leftSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Left };
        leftSwipe.AddTarget(() => {
            Logger.Debug($"Left swipe detected, current index: {_viewModel.SelectedIndex}");
            if (_viewModel.SelectedIndex < _viewModel.AvailableDays.Count - 1) {
                _viewModel.SelectedIndex++;
                Logger.Debug($"Changed index to {_viewModel.SelectedIndex}");
            }
        });
        _menusTable.AddGestureRecognizer(leftSwipe);

        // Add swipe gesture for navigating to previous day
        var rightSwipe = new UISwipeGestureRecognizer { Direction = UISwipeGestureRecognizerDirection.Right };
        rightSwipe.AddTarget(() => {
            Logger.Debug($"Right swipe detected, current index: {_viewModel.SelectedIndex}");
            if (_viewModel.SelectedIndex > 0) {
                _viewModel.SelectedIndex--;
                Logger.Debug($"Changed index to {_viewModel.SelectedIndex}");
            }
        });
        _menusTable.AddGestureRecognizer(rightSwipe);
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
        Logger.Debug($"OnSafeAreaChanged called, insets: Top={safeArea.Top}, Bottom={safeArea.Bottom}, Left={safeArea.Left}, Right={safeArea.Right}");

        // Adjust page control position
        _pageControl!.Frame = new CGRect(0, safeArea.Top, View.Bounds.Width, 40);

        // Adjust date label position
        _dateLabel!.Frame = new CGRect(16, safeArea.Top + 40, View.Bounds.Width - 32, 40);

        // Adjust table view position and size
        var tableY = safeArea.Top + 80;
        var tableHeight = View.Bounds.Height - safeArea.Top - 80 - safeArea.Bottom;
        _menusTable!.Frame = new CGRect(0, tableY, View.Bounds.Width, tableHeight);
        _menusTable!.RowHeight = tableHeight / 4;
    }

    /// <summary>
    /// Updates the UI to reflect the current state of the view model.
    /// </summary>
    private void UpdateUI()
    {
        Logger.Debug($"UpdateUI called, selected index: {_viewModel.SelectedIndex}, available days: {_viewModel.AvailableDays.Count}");

        // Update date label
        if (_viewModel.AvailableDays.Count > _viewModel.SelectedIndex)
        {
            var selectedDay = _viewModel.AvailableDays[_viewModel.SelectedIndex];
            _dateLabel!.Text = selectedDay.Date.ToString("dddd, MMMM d, yyyy");
        }
        else
        {
            _dateLabel!.Text = "";
        }

        // Reload table data and update page control
        _menusTable!.ReloadData();
        _pageControl!.Pages = _viewModel.AvailableDays.Count;
        _pageControl!.CurrentPage = _viewModel.SelectedIndex;

        // Debug logging for key window
        var keyWindow = UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .LastOrDefault(w => w.IsKeyWindow);
        Logger.Debug($"UIApplication keyWindow = {keyWindow}");
    }

    /// <summary>
    /// Handler for Info events from GC.Common.Base. Shows a brief alert with the context.
    /// Runs on the main thread because it updates UI.
    /// </summary>
    private void HandleGlobalInfo(object? sender, InfoEventArgs? e)
    {
        if (e == null) return;
        var message = e.Context ?? e.Type.ToString();
        // Use the in-app non-modal banner
        GC.iOS.Helpers.InAppNotifier.ShowInfo(message);
    }

    /// <summary>
    /// Handler for Error events from GC.Common.Base. Shows an alert describing the error.
    /// Runs on the main thread because it updates UI.
    /// </summary>
    private void HandleGlobalError(object? sender, ErrorEventArgs? e)
    {
        if (e == null) return;
        var message = e.Exception?.Message ?? e.Context ?? e.Type.ToString();
        GC.iOS.Helpers.InAppNotifier.ShowError(message);
    }

    /// <summary>
    /// Dispose override to unsubscribe from static events and avoid leaks.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GC.Common.Base.OnInfo -= HandleGlobalInfo;
            GC.Common.Base.OnError -= HandleGlobalError;
        }
        base.Dispose(disposing);
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
        if (_viewModel.AvailableDays.Count > _viewModel.SelectedIndex)
        {
            return _viewModel.AvailableDays[_viewModel.SelectedIndex].Menus.Count;
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
}
