// Converters/DrawingColorConverter.cs
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Drawing;
using System.Globalization;

namespace graphic_editor.Converters; 

public class DrawingColorConverter : IValueConverter
{
    public static readonly DrawingColorConverter Instance = new();
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Drawing.Color c)
            return Avalonia.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        return Colors.Black;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Avalonia.Media.Color c)
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        return System.Drawing.Color.Black;
    }
}