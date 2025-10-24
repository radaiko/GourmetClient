using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace GC.Frontend.Mobile.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
[ContentProperty("Content")]
public partial class GroupToggle : ContentView {
  
  public static readonly BindableProperty LabelProperty = BindableProperty.Create(
    nameof(Label),
    typeof(string),
    typeof(GroupToggle),
    string.Empty);

  public string Label {
    get => (string)GetValue(LabelProperty);
    set => SetValue(LabelProperty, value);
  }
  
  public static readonly BindableProperty ToggleProperty = BindableProperty.Create(
    nameof(Toggle),
    typeof(bool),
    typeof(GroupToggle),
    false);

  public bool Toggle {
    get => (bool)GetValue(ToggleProperty);
    set => SetValue(ToggleProperty, value);
  }
  
  public GroupToggle() {
    InitializeComponent();
  }
}