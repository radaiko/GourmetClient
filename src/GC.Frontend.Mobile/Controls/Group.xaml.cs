using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace GC.Frontend.Mobile.Controls;

public partial class Group {
  
  public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
    nameof(Header),
    typeof(string),
    typeof(Group),
    string.Empty);

  public string Header {
    get => (string)GetValue(HeaderProperty);
    set => SetValue(HeaderProperty, value);
  }

  public Group() {
    InitializeComponent();

    // Diagnostic: try to locate the diagnostic label placed in XAML and make it obviously visible at runtime
    try {
      var diag = this.FindByName<Label>("DiagSep");
      Debug.WriteLine($"Group ctor: DiagSep found? {diag != null}");
      Debug.WriteLine($"Group ctor: Header='{Header}'");
      if (diag != null) {
        diag.Text = "DIAG-SEP (runtime)";
        diag.BackgroundColor = Colors.Magenta;
        diag.TextColor = Colors.White;
        diag.IsVisible = true;
        diag.HeightRequest = 36;
      }
    }
    catch (Exception ex) {
      Debug.WriteLine($"Group ctor diagnostic error: {ex}");
    }
  }
}