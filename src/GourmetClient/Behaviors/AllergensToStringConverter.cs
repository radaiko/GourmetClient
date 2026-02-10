using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace GourmetClient.Behaviors;

/// <summary>
/// An implementation of the <see cref="IValueConverter"/> interface which provides a string representation of the
/// allergens of a menu.
/// </summary>
public class AllergensToStringConverter : IValueConverter
{
    /// <summary>
    /// Converts the allergens of a menu to its string representation.
    /// </summary>
    /// <param name="value">The allergens of the menu. Must be of type <see cref="IEnumerable{T}"/>.</param>
    /// <param name="targetType">This parameter is not used.</param>
    /// <param name="parameter">This parameter is not used.</param>
    /// <param name="culture">This parameter is not used.</param>
    /// <returns></returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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