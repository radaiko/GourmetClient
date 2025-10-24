using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GC.Frontend.Mobile.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
[ContentProperty("Content")]
public partial class Group : ContentView {
  
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