using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace GourmetClient.Behaviors;

public class AllergensToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<char> allergenList)
        {
            return string.Join(", ", allergenList);
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}