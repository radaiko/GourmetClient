using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;

namespace GourmetClient.Maui.Behaviors;

public class AllergensToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable allergens)
        {
            return string.Empty;
        }

        var allergensList = allergens.Cast<object>().ToList();
        if (allergensList.Count == 0)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();
        foreach (var allergen in allergensList)
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(", ");
            }
            stringBuilder.Append(allergen.ToString());
        }

        return stringBuilder.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}