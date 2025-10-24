using UIKit;
using GC.Frontend.ViewModels;

namespace GC.iOS.Controllers;

public class SettingsViewController : UIViewController, IUITableViewDataSource {
  private const UITextBorderStyle RoundedRect = UITextBorderStyle.RoundedRect;
  
  private UITableView? _table;
  private UITextField? _gourmetUsername;
  private UITextField? _gourmetPassword;
  private UITextField? _ventoUsername;
  private UITextField? _ventoPassword;
  private UISwitch? _debugSwitch;
  private UIStackView? _debugToggle;
  private SettingsViewModel _viewModel = new();
  
  public override void ViewDidLoad() {
    base.ViewDidLoad();
    _table = new UITableView(View.Bounds, UITableViewStyle.InsetGrouped) {
      AutoresizingMask = UIViewAutoresizing.All,
      BackgroundColor = UIColor.SystemBackground,
      RowHeight = Common.StandardCellHeight
    };
    _table.DataSource = this;
    View.AddSubview(_table);
    _gourmetUsername = CreateTextField("Benutzername", false);
    _gourmetPassword = CreateTextField("Password", true);
    _ventoUsername = CreateTextField("Benutzername", false);
    _ventoPassword = CreateTextField("Password", true);
    _debugSwitch = new UISwitch();
    _debugToggle = CreateToggle("Debug Modus", _debugSwitch);
    
    // Set initial values from ViewModel
    _gourmetUsername.Text = _viewModel.GourmetUsername;
    _gourmetPassword.Text = _viewModel.GourmetPassword;
    _ventoUsername.Text = _viewModel.VentoUsername;
    _ventoPassword.Text = _viewModel.VentoPassword;
    _debugSwitch.On = _viewModel.DebugMode;
    
    // Add event handlers
    _gourmetUsername.EditingChanged += (_, _) => _viewModel.GourmetUsername = _gourmetUsername.Text;
    _gourmetPassword.EditingChanged += (_, _) => _viewModel.GourmetPassword = _gourmetPassword.Text;
    _ventoUsername.EditingChanged += (_, _) => _viewModel.VentoUsername = _ventoUsername.Text;
    _ventoPassword.EditingChanged += (_, _) => _viewModel.VentoPassword = _ventoPassword.Text;
    _debugSwitch.ValueChanged += (_, _) => _viewModel.DebugMode = _debugSwitch.On;
    
    _table.ReloadData();
  }
  
  private static UITextField CreateTextField(string placeholder, bool isSecure) {
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

  private static UIStackView CreateToggle(string labelText, UISwitch toggleSwitch) {
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

  public nint NumberOfSections(UITableView tableView) => 3;

  public string? TitleForHeader(UITableView tableView, nint section) => section switch {
    0 => "Gourmet",
    1 => "Ventopay",
    2 => "Allgemein",
    _ => null
  };

  public nint RowsInSection(UITableView tableView, nint section) => section < 2 ? 2 : 1;

  public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath) {
    var cell = new UITableViewCell(UITableViewCellStyle.Default, null);
    switch (indexPath.Section) {
      case 0 when indexPath.Row == 0:
        _gourmetUsername!.Frame = new CGRect(Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Common.StandardCellHeight);
        cell.ContentView.AddSubview(_gourmetUsername);
        break;
      case 0:
        _gourmetPassword!.Frame = new CGRect(Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Common.StandardCellHeight);
        cell.ContentView.AddSubview(_gourmetPassword);
        break;
      case 1 when indexPath.Row == 0:
        _ventoUsername!.Frame = new CGRect(Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Common.StandardCellHeight);
        cell.ContentView.AddSubview(_ventoUsername);
        break;
      case 1:
        _ventoPassword!.Frame = new CGRect(Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Common.StandardCellHeight);
        cell.ContentView.AddSubview(_ventoPassword);
        break;
      case 2:
        _debugToggle!.Frame = new CGRect(Common.StandardMargin, 0, cell.ContentView.Bounds.Width, Common.StandardCellHeight);
        cell.ContentView.AddSubview(_debugToggle);
        break;
    }
    return cell;
  }
}