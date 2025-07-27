using System.Windows;

namespace GourmetClient.Behaviors;

public class BoolToVisibilityConverter : BoolConverterBase<Visibility>
{
    public BoolToVisibilityConverter()
        : base(trueValue: Visibility.Visible, falseValue: Visibility.Collapsed)
    {
    }
}