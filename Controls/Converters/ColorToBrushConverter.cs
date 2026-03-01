// Controls/Converters/ColorToBrushConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace graphic_editor.Converters;
public class ColorToBrushConverter : IValueConverter
{
    public static readonly ColorToBrushConverter Instance = new();
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.Color color)
            return new SolidColorBrush(
                Color.FromArgb(color.A, color.R, color.G, color.B));
        return Brushes.Black;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return System.Drawing.Color.FromArgb(
                brush.Color.A,
                brush.Color.R,
                brush.Color.G,
                brush.Color.B);
        return System.Drawing.Color.Black;
    }
}