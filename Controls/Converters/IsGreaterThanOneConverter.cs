using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace graphic_editor.Converters;

public class IsGreaterThanOneConverter : IValueConverter
{
    public static readonly IsGreaterThanOneConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int count && count > 1;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}