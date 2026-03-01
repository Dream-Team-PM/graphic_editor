// Controls/Converters/PercentToDoubleConverter.cs
using Avalonia.Data.Converters;
using System.Globalization;

namespace graphic_editor.Converters;

public class PercentToDoubleConverter : IValueConverter
{
    public static readonly PercentToDoubleConverter Instance = new();
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
            return percent / 100.0;  // 100 → 1.0
        return 1.0;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double opacity)
            return opacity * 100.0;  // 1.0 → 100
        return 100.0;
    }
}