using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using GC.Core.Utils;

namespace GC.Views.Controls;

public class IndicatorDots : UserControl {
  #region Fields ---------------------------------------------------------------
  private readonly StackPanel _dotsPanel;
  #endregion

  #region Properties -----------------------------------------------------------
  public static readonly StyledProperty<int> TotalProperty =
    AvaloniaProperty.Register<IndicatorDots, int>(nameof(Total), 0);

  public static readonly StyledProperty<int> CurrentIndexProperty =
    AvaloniaProperty.Register<IndicatorDots, int>(nameof(CurrentIndex), 0);

  public int Total { get => GetValue(TotalProperty); set => SetValue(TotalProperty, value); }

  public int CurrentIndex { get => GetValue(CurrentIndexProperty); set => SetValue(CurrentIndexProperty, value); }

  public event EventHandler<int>? DotClicked;
  
  #endregion

  #region Constructors ---------------------------------------------------------
  public IndicatorDots() {
    Log.Write("Constructor called");
    _dotsPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 6
    };
    Content = _dotsPanel;
    UpdateDots();
  }
  #endregion

  #region Methods --------------------------------------------------------------
  protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
    base.OnPropertyChanged(change);
    Log.Write($"Property changed: {change.Property.Name}");
    if (change.Property == TotalProperty || change.Property == CurrentIndexProperty) {
      UpdateDots();
    }
  }

  private void UpdateDots() {
    Log.Write($"Updating dots: Total={Total}, CurrentIndex={CurrentIndex}");
    _dotsPanel.Children.Clear();
    for (var i = 0; i < Total; i++) {
      var isActive = i == CurrentIndex;
      var dot = new Ellipse {
        Width = isActive ? 10 : 8,
        Height = isActive ? 10 : 8,
        Fill = isActive
          ? new SolidColorBrush(Color.FromRgb(255, 255, 255))
          : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
        StrokeThickness = 0
      };
      var index = i; // Capture the current index in a local variable
      dot.PointerPressed += (s, e) => {
        CurrentIndex = index;
        DotClicked?.Invoke(this, index);
      };
      _dotsPanel.Children.Add(dot);
    }
  }
  #endregion
}