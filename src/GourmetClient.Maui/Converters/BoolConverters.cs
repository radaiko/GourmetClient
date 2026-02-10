using System.Globalization;

namespace GourmetClient.Maui.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string colorParam)
            return Colors.Transparent;

        var colors = colorParam.Split('|');
        if (colors.Length != 2)
            return Colors.Transparent;

        var colorKey = boolValue ? colors[0] : colors[1];

        // Try to get from app resources
        if (Application.Current?.Resources.TryGetValue(colorKey, out var resourceColor) == true && resourceColor is Color color)
        {
            return color;
        }

        // Fallback to parsing color
        return colorKey.ToLowerInvariant() switch
        {
            "white" => Colors.White,
            "transparent" => Colors.Transparent,
            _ => Color.FromArgb(colorKey)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
