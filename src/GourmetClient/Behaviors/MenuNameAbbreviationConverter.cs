using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace GourmetClient.Behaviors;

public partial class MenuNameAbbreviationConverter : IValueConverter
{
    [GeneratedRegex(@"(MENÜ\s+[I]{1,3})")]
    private static partial Regex MenuTitleRegex();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            var match = MenuTitleRegex().Match(stringValue);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}