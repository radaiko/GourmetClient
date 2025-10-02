using Microsoft.Maui.Controls;

namespace GourmetClient.Maui.Behaviors;

public class BoolToVisibilityConverter : BoolConverterBase<bool>
{
    public BoolToVisibilityConverter()
    {
        TrueValue = true;  // Visible in MAUI corresponds to IsVisible = true
        FalseValue = false; // Hidden/Collapsed in MAUI corresponds to IsVisible = false
    }
}