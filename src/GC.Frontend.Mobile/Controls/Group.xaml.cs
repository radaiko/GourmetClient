using Microsoft.Maui.Controls;

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
  }
}