// Converters/IsZeroToVisibleConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

public class IsZeroToVisibleConverter : IValueConverter
{
    public static readonly IsZeroToVisibleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int count && count == 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}