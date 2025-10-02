using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GourmetClient.Maui.Behaviors;

public abstract class BoolConverterBase<T> : IValueConverter
{
    public T TrueValue { get; set; } = default!;
    public T FalseValue { get; set; } = default!;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TrueValue : FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}