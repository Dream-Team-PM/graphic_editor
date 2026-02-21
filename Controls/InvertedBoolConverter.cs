// Converters/InvertedBoolConverter.cs

using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public static readonly InvertedBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}