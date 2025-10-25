using UIKit;
using GC.Frontend.ViewModels;

namespace GC.iOS.Controllers;

/// <summary>
/// View controller for displaying and editing application settings.
/// Includes login credentials for Gourmet and Ventopay services, and debug options.
/// </summary>
public class SettingsViewController : UIViewController, IUITableViewDataSource
{
    /// <summary>
    /// Constant for rounded rect border style (kept for compatibility).
    /// </summary>
    private const UITextBorderStyle RoundedRect = UITextBorderStyle.RoundedRect;

    /// <summary>
    /// Table view displaying the settings sections.
    /// </summary>
    private UITableView? _table;

    /// <summary>
    /// Text field for Gourmet username.
    /// </summary>
    private UITextField? _gourmetUsername;

    /// <summary>
    /// Text field for Gourmet password.
    /// </summary>
    private UITextField? _gourmetPassword;

    /// <summary>
    /// Text field for Ventopay username.
    /// </summary>
    private UITextField? _ventoUsername;

    /// <summary>
    /// Text field for Ventopay password.
    /// </summary>
    private UITextField? _ventoPassword;

    /// <summary>
    /// Switch for toggling debug mode.
    /// </summary>
    private UISwitch? _debugSwitch;

    /// <summary>
    /// Stack view containing the debug toggle label and switch.
    /// </summary>
    private UIStackView? _debugToggle;

    /// <summary>
    /// View model that manages the settings data.
    /// </summary>
    private SettingsViewModel _viewModel = new();

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Sets up the table view and creates all the settings controls.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Create the main table view
        _table = new UITableView(View.Bounds, UITableViewStyle.InsetGrouped) {
            AutoresizingMask = UIViewAutoresizing.All,
            BackgroundColor = UIColor.SystemBackground,
            RowHeight = Helpers.Common.StandardCellHeight
        };
        _table.DataSource = this;
        View.AddSubview(_table);

        // Create text fields for credentials
        _gourmetUsername = CreateTextField("Benutzername", false);
        _gourmetPassword = CreateTextField("Password", true);
        _ventoUsername = CreateTextField("Benutzername", false);
        _ventoPassword = CreateTextField("Password", true);

        // Create debug toggle
        _debugSwitch = new UISwitch();
        _debugToggle = CreateToggle("Debug Modus", _debugSwitch);

        // Set initial values from the view model
        _gourmetUsername.Text = _viewModel.GourmetUsername;
        _gourmetPassword.Text = _viewModel.GourmetPassword;
        _ventoUsername.Text = _viewModel.VentoUsername;
        _ventoPassword.Text = _viewModel.VentoPassword;
        _debugSwitch.On = _viewModel.DebugMode;

        // Add event handlers to update view model when controls change
        _gourmetUsername.EditingChanged += (_, _) => _viewModel.GourmetUsername = _gourmetUsername.Text;
        _gourmetPassword.EditingChanged += (_, _) => _viewModel.GourmetPassword = _gourmetPassword.Text;
        _ventoUsername.EditingChanged += (_, _) => _viewModel.VentoUsername = _ventoUsername.Text;
        _ventoPassword.EditingChanged += (_, _) => _viewModel.VentoPassword = _ventoPassword.Text;
        _debugSwitch.ValueChanged += (_, _) => _viewModel.DebugMode = _debugSwitch.On;

        // Load the table data
        _table.ReloadData();
    }

    /// <summary>
    /// Creates a styled text field for input.
    /// </summary>
    /// <param name="placeholder">The placeholder text to display.</param>
    /// <param name="isSecure">Whether this is a secure text entry field (password).</param>
    /// <returns>The configured text field.</returns>
    private static UITextField CreateTextField(string placeholder, bool isSecure)
    {
        var tf = new UITextField {
            Placeholder = placeholder,
            BorderStyle = UITextBorderStyle.None,
            SecureTextEntry = isSecure,
            TextColor = UIColor.Label,
            BackgroundColor = new UIColor(white: 1, alpha: 0f),
            AttributedPlaceholder = new NSAttributedString(placeholder, new UIStringAttributes { ForegroundColor = UIColor.PlaceholderText }),
        };
        return tf;
    }

    /// <summary>
    /// Creates a horizontal stack view with a label and switch for toggles.
    /// </summary>
    /// <param name="labelText">The text for the label.</param>
    /// <param name="toggleSwitch">The switch control.</param>
    /// <returns>The configured stack view.</returns>
    private static UIStackView CreateToggle(string labelText, UISwitch toggleSwitch)
    {
        var label = new UILabel {
            Text = labelText,
            TextAlignment = UITextAlignment.Left,
        };
        label.SetContentCompressionResistancePriority(751, UILayoutConstraintAxis.Horizontal);
        toggleSwitch.SetContentHuggingPriority(251, UILayoutConstraintAxis.Horizontal);
        var stackView = new UIStackView([label, toggleSwitch]) {
            Axis = UILayoutConstraintAxis.Horizontal,
            Alignment = UIStackViewAlignment.Center,
        };
        return stackView;
    }

    /// <summary>
    /// Returns the number of sections in the table view.
    /// </summary>
    /// <param name="tableView">The table view requesting the information.</param>
    /// <returns>The number of sections (3: Gourmet, Ventopay, General).</returns>
    public nint NumberOfSections(UITableView tableView) => 3;

    /// <summary>
    /// Returns the title for the specified section header.
    /// </summary>
    /// <param name="tableView">The table view requesting the title.</param>
    /// <param name="section">The section index.</param>
    /// <returns>The header title for the section.</returns>
    public string? TitleForHeader(UITableView tableView, nint section) => section switch {
        0 => "Gourmet",
        1 => "Ventopay",
        2 => "Allgemein",
        _ => null
    };

    /// <summary>
    /// Returns the number of rows in the specified section.
    /// </summary>
    /// <param name="tableView">The table view requesting the information.</param>
    /// <param name="section">The section index.</param>
    /// <returns>The number of rows in the section.</returns>
    public nint RowsInSection(UITableView tableView, nint section) => section < 2 ? 2 : 1;

    /// <summary>
    /// Returns a cell for the specified index path.
    /// </summary>
    /// <param name="tableView">The table view requesting the cell.</param>
    /// <param name="indexPath">The index path of the cell.</param>
    /// <returns>The configured table view cell.</returns>
    public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
    {
        var cell = new UITableViewCell(UITableViewCellStyle.Default, null);
        switch (indexPath.Section)
        {
            case 0 when indexPath.Row == 0:
                // Gourmet username field
                _gourmetUsername!.Frame = new CGRect(Helpers.Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Helpers.Common.StandardCellHeight);
                cell.ContentView.AddSubview(_gourmetUsername);
                break;
            case 0:
                // Gourmet password field
                _gourmetPassword!.Frame = new CGRect(Helpers.Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Helpers.Common.StandardCellHeight);
                cell.ContentView.AddSubview(_gourmetPassword);
                break;
            case 1 when indexPath.Row == 0:
                // Ventopay username field
                _ventoUsername!.Frame = new CGRect(Helpers.Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Helpers.Common.StandardCellHeight);
                cell.ContentView.AddSubview(_ventoUsername);
                break;
            case 1:
                // Ventopay password field
                _ventoPassword!.Frame = new CGRect(Helpers.Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Helpers.Common.StandardCellHeight);
                cell.ContentView.AddSubview(_ventoPassword);
                break;
            case 2:
                // Debug toggle
                _debugToggle!.Frame = new CGRect(Helpers.Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Helpers.Common.StandardCellHeight);
                cell.ContentView.AddSubview(_debugToggle);
                break;
        }
        return cell;
    }
}