// Converters/IsGreaterThanZeroConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

public class IsGreaterThanZeroConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
            return intValue > 0;
        if (value is double doubleValue)
            return doubleValue > 0;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}