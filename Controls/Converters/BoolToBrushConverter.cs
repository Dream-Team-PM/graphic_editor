// Controls/Converters/BoolToBrushConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace graphic_editor.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public Brush VisibleBrush { get; set; } = new SolidColorBrush(Colors.LimeGreen);
    public Brush HiddenBrush { get; set; } = new SolidColorBrush(Colors.Gray);
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible ? VisibleBrush : HiddenBrush;
        }
        return HiddenBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}