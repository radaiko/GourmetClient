using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GC.Views;

/// <summary>
/// Simple boolean inverter converter for Avalonia bindings.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter {
  public static readonly InverseBoolConverter Instance = new();

  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
    if (value is bool b)
      return !b;
    return value; // pass through if not bool
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
    if (value is bool b)
      return !b;
    return value;
  }
}