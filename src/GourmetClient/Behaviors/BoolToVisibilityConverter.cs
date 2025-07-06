using System.Windows;

namespace GourmetClient.Behaviors;

public class BoolToVisibilityConverter : BoolConverterBase<Visibility>
{
    public BoolToVisibilityConverter()
        : base(Visibility.Visible, Visibility.Collapsed)
    {
    }
}